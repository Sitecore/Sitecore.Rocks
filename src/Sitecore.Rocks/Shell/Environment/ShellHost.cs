// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;
using Sitecore.Rocks.Shell.Pipelines.Initialization;

namespace Sitecore.Rocks.Shell.Environment
{
    public class ShellHost
    {
        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x00010000;

        private const int WS_MINIMIZEBOX = 0x00020000;

        private static readonly DispatcherOperationCallback ExitFrameCallback = ExitFrame;

        public ShellHost()
        {
            VisualStudioVersion = new Version(0, 0, 0, 0);
            VisualStudioLocation = string.Empty;
            SitecoreRocksVersion = Assembly.GetExecutingAssembly().GetFileVersion();
            ShellIdentifier = Constants.SitecoreRocksVisualStudio;
        }

        [NotNull]
        public virtual string[] CommandLineArgs => CommandLineToArgs(CommandLineArguments);

        [NotNull]
        public virtual string CommandLineArguments => System.Environment.CommandLine;

        [NotNull]
        public string ShellIdentifier { get; set; }

        [NotNull]
        public virtual Version SitecoreRocksVersion { get; private set; }

        [NotNull]
        public string VisualStudioLocation { get; set; }

        public AppTheme VisualStudioTheme { get; set; }

        [NotNull]
        public Version VisualStudioVersion { get; set; }

        public virtual void DoEvents()
        {
            var nestedFrame = new DispatcherFrame();

            var exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, ExitFrameCallback, nestedFrame);

            try
            {
                Dispatcher.PushFrame(nestedFrame);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            if (exitOperation.Status != DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
        }

        public virtual bool DoEvents(ref bool busy, int timeoutInSeconds)
        {
            var timeOut = DateTime.UtcNow.AddSeconds(timeoutInSeconds);

            while (busy)
            {
                DoEvents();

                if (DateTime.UtcNow > timeOut)
                {
                    busy = false;
                    return false;
                }
            }

            return true;
        }

        public virtual bool DoEvents(ref int busy, int timeoutInSeconds)
        {
            var timeOut = DateTime.UtcNow.AddSeconds(timeoutInSeconds);

            while (busy > 0)
            {
                DoEvents();

                if (DateTime.UtcNow > timeOut)
                {
                    busy = 0;
                    return false;
                }
            }

            return true;
        }

        public virtual Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();

            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public virtual void HandleException([NotNull] Exception exception)
        {
            Assert.ArgumentNotNull(exception, nameof(exception));

            var friendlyException = exception as FriendlyException;
            if (friendlyException != null)
            {
                if (friendlyException.Handle())
                {
                    return;
                }
            }

            if (exception is MissingMethodException && exception.StackTrace.IndexOf(@"HedgehogDevelopment.SitecoreRocks.Diagrammer", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MessageBox("The Hedgehog Template Diagrammer plugin is out of date.\n\nPlease upgrade the plugin.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public virtual void HandleException([NotNull] object sender, [NotNull] DispatcherUnhandledExceptionEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            var shutDownException = e.Exception as InvalidOperationException;
            if (shutDownException != null && shutDownException.Message.Contains("The Application object is being shut down."))
            {
                return;
            }

            HandleException(e.Exception);
        }

        public void Initialize()
        {
            // DeferredPluginManager.UninstallPlugins();
            // DeferredPluginManager.InstallPlugins();
            AppHost.Plugins.Uninstall();

            ExtensibilityLoader.Initialize();

            AppHost.Usage.Report(string.Format(@"Version {0}", Assembly.GetExecutingAssembly().GetFileVersion()));

            InitializationPipeline.Run().WithParameters(true);
        }

        public virtual void InitializeDialog([NotNull] Window dialog)
        {
            Assert.ArgumentNotNull(dialog, nameof(dialog));

            var application = Application.Current;
            if (application != null && !Equals(application.MainWindow, dialog))
            {
                dialog.Owner = application.MainWindow;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowInTaskbar = false;
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            dialog.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            dialog.SetValue(TextOptions.TextRenderingModeProperty, TextRenderingMode.Auto);

            if (dialog.ResizeMode != ResizeMode.NoResize)
            {
                dialog.Loaded += DialogLoaded;
            }
        }

        public virtual MessageBoxResult MessageBox([NotNull] string text, [NotNull] string title, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxIcon)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(title, nameof(title));

            var application = Application.Current;
            if (application != null)
            {
                var mainWindow = application.MainWindow;
                if (mainWindow != null)
                {
                    return System.Windows.MessageBox.Show(mainWindow, text, title, messageBoxButton, messageBoxIcon);
                }
            }

            return System.Windows.MessageBox.Show(text, title, messageBoxButton, messageBoxIcon);
        }

        [CanBeNull]
        public virtual string Prompt([NotNull] string text, [NotNull] string title, [NotNull] string value)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(value, nameof(value));

            return UI.Dialogs.Prompts.Prompt.Show(text, title, value);
        }

        public virtual void SaveActiveDocument()
        {
        }

        public virtual bool? ShowDialog([NotNull] Window dialog)
        {
            Assert.ArgumentNotNull(dialog, nameof(dialog));

            try
            {
                return dialog.ShowDialog();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Cannot set Visibility or call Show, ShowDialog, or WindowInteropHelper.EnsureHandle after a Window has closed."))
                {
                    return null;
                }

                if (ex.Message.Contains("Cannot perform requested operation because the Dispatcher shut down."))
                {
                    return null;
                }

                throw;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [NotNull]
        private string[] CommandLineToArgs([NotNull] string commandLine)
        {
            Debug.ArgumentNotNull(commandLine, nameof(commandLine));

            int argc;
            var argv = CommandLineToArgvW(@"foo.exe " + commandLine, out argc);
            if (argv == IntPtr.Zero)
            {
                return new string[0];
            }

            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        [DllImport(@"shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string commandLine, out int argsCount);

        private void DialogLoaded([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));
            Debug.ArgumentNotNull(sender, nameof(sender));

            var dialog = sender as Window;
            if (dialog == null)
            {
                return;
            }

            dialog.Loaded -= DialogLoaded;

            var hwnd = new WindowInteropHelper(dialog).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~(WS_MINIMIZEBOX | WS_MAXIMIZEBOX));
        }

        [CanBeNull]
        private static object ExitFrame([CanBeNull] object state)
        {
            var frame = state as DispatcherFrame;
            if (frame == null)
            {
                return null;
            }

            frame.Continue = false;

            return null;
        }

        [DllImport(@"user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport(@"user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newLong);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;

            public int Y;
        }
    }
}
