// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.Options;
using Sitecore.Rocks.Searching;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Shell.Commands;
using Sitecore.Rocks.Shell.Environment;
using Sitecore.Rocks.Shell.Pipelines.ShellLoaded;
using Sitecore.Rocks.UI;
using Sitecore.Rocks.UI.Archives;
using Sitecore.Rocks.UI.EditorWindowHosts;
using Sitecore.Rocks.UI.JobViewer;
using Sitecore.Rocks.UI.Links;
using Sitecore.Rocks.UI.LogViewer;
using Sitecore.Rocks.UI.Management;
using Sitecore.Rocks.UI.Packages.PackageBuilders;
using Sitecore.Rocks.UI.PublishingQueue;
using Sitecore.Rocks.UI.QueryAnalyzer;
using Sitecore.Rocks.UI.SearchAndReplace;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.TemplateDesigner;
using Sitecore.Rocks.UI.TemplateFieldSorter;
using Sitecore.Rocks.UI.TemplateHierarchies;
using Sitecore.Rocks.UI.ToolWindowPaneHosts;
using Sitecore.Rocks.UI.ValidationIssues;
using Sitecore.Rocks.UI.XpathBuilder;

namespace Sitecore.Rocks
{
    // ReSharper disable ArrangeAttributes
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(@"#110", @"#112", @"1.0", IconResourceID = 400)]
    [ProvideMenuResource(@"Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(ArchivePane), Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(ContentTreePane))]
    [ProvideToolWindow(typeof(ContentEditorPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(JobViewerPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(LinkPane), MultiInstances = false, Style = VsDockStyle.Float, Transient = true)]
    [ProvideToolWindow(typeof(LogViewerPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(ManagementViewerPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(MediaPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(QueryAnalyzerToolPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(XpathBuilderToolPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(PublishingQueuePane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(RecycleBinPane), Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(SearchAndReplacePane), MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(SearchPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(TemplateDesignerPane), MultiInstances = true, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(TemplateHierarchyPane), MultiInstances = false, Style = VsDockStyle.Float, Transient = true)]
    [ProvideToolWindow(typeof(ToolWindowPaneHost), MultiInstances = true, Style = VsDockStyle.Float, Transient = true)]
    [ProvideToolWindow(typeof(ValidationIssuesPane), MultiInstances = false, Style = VsDockStyle.Float, Transient = true)]
    [ProvideToolWindow(typeof(StartPagePane), MultiInstances = false, Style = VsDockStyle.MDI, Transient = true)]
    [ProvideOptionPage(typeof(KeyboardSchemeOptions), @"Sitecore", @"Keyboard", 1000, 1005, false)]
    [ProvideOptionPage(typeof(MacroOptions), @"Sitecore", @"Macros", 1000, 1002, false)]
    [ProvideOptionPage(typeof(VisualStudioOptions), @"Sitecore", @"Options", 1000, 1003, false)]
    [ProvideEditorFactory(typeof(ContentEditorFactory), 200, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorExtension(typeof(ContentEditorFactory), ContentEditorFileExtension, 32, NameResourceID = 200)]
    [ProvideEditorLogicalView(typeof(ContentEditorFactory), GuidList.ContentEditorLogicalView)]
    [ProvideEditorFactory(typeof(TemplateDesignerFactory), 201, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorExtension(typeof(TemplateDesignerFactory), TemplateDesignerFactoryFileExtension, 32, NameResourceID = 201)]
    [ProvideEditorLogicalView(typeof(TemplateDesignerFactory), GuidList.TemplateDesignerLogicalView)]
    [ProvideEditorFactory(typeof(TemplateFieldSorterFactory), 202, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorExtension(typeof(TemplateFieldSorterFactory), TemplateDesignerFactoryFileExtension, 32, NameResourceID = 202)]
    [ProvideEditorLogicalView(typeof(TemplateFieldSorterFactory), GuidList.TemplateFieldSorterLogicalView)]
    [ProvideEditorFactory(typeof(EditorWindowFactory), 205, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorExtension(typeof(EditorWindowFactory), EditorWindowFactoryFileExtension, 32, NameResourceID = 205)]
    [ProvideEditorLogicalView(typeof(EditorWindowFactory), GuidList.EditorWindowLogicalView)]
    [ProvideEditorFactory(typeof(CodeGenerationFactory), 206, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorExtension(typeof(CodeGenerationFactory), CodeGenerationFileExtension, 1, NameResourceID = 206)]
    [ProvideEditorLogicalView(typeof(CodeGenerationFactory), GuidList.CodeGenerationLogicalView)]
    [ProvideEditorFactory(typeof(PackageBuilderFactory), 206, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorExtension(typeof(PackageBuilderFactory), PackageBuilderExtension, 1, NameResourceID = 206)]
    [ProvideEditorLogicalView(typeof(PackageBuilderFactory), GuidList.PackageBuilderLogicalView)]
    [ProvideXmlEditorChooserDesignerView("Sitecore Package", "package", LogicalViewID.Designer, 0x61, DesignerLogicalViewEditor = typeof(PackageBuilderFactory), Namespace = @"http://schemas.sitecore.com/codegeneration/2010", MatchExtensionAndNamespace = false)]
    [ProvideXmlEditorChooserDesignerView("Sitecore Code Generation", "scx", LogicalViewID.Designer, 0x60, DesignerLogicalViewEditor = typeof(CodeGenerationFactory), Namespace = @"http://schemas.sitecore.com/codegeneration/2010", MatchExtensionAndNamespace = false)]
    [Guid(GuidList.VisualStudioPackageString)]
    [UsedImplicitly]

    // ReSharper restore ArrangeAttributes
    public sealed class SitecorePackage : AsyncPackage, IVsShellPropertyEvents
    {
        public const string CodeGenerationFileExtension = ".scx";

        public const string ContentEditorFileExtension = ".sitecore.item";

        public const string EditorWindowFactoryFileExtension = ".sitecore.editorwindow";

        public const string LayoutDesignerFactoryFileExtension = ".sitecore.layout";

        public const string PackageBuilderExtension = ".package";

        public const string TemplateDesignerFactoryFileExtension = ".sitecore.template";

        public const string XsltEditorFileExtension = ".sitecore.xslt";

        private uint _eventSinkCookie;

        [NotNull]
        public BuildEvents BuildEvents { get; set; }

        [NotNull]
        public DTE2 Dte => (DTE2)GetService(typeof(SDTE));

        [NotNull]
        public Events2 Events { get; set; }

        [NotNull]
        public static SitecorePackage Instance { get; set; }

        [NotNull]
        public OleMenuCommandService MenuService => (OleMenuCommandService)GetService(typeof(IMenuCommandService));

        [NotNull]
        public VisualStudioOptions Options => (VisualStudioOptions)GetDialogPage(typeof(VisualStudioOptions));

        [NotNull]
        public ProjectItemsEvents ProjectItemsEvents { get; set; }

        [NotNull]
        public SolutionEvents SolutionEvents { get; set; }

        [NotNull]
        public VsRunningDocumentTable VsRunningDocumentTable { get; set; }

        [NotNull]
        public VsTrackProjectDocuments VsTrackProjectDocument
        {
            [UsedImplicitly]
            get;
            set;
        }

        [NotNull]
        public VsTrackSelection VsTrackSelection
        {
            [UsedImplicitly]
            get;
            set;
        }

        [NotNull]
        public WindowEvents WindowEvents { get; set; }

        [CanBeNull]
        public T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }

        [CanBeNull]
        public IVsTextManager GetTextManagerService()
        {
            return GetService(typeof(VsTextManagerClass)) as IVsTextManager;
        }

        public void GotFocus([NotNull] IPane pane)
        {
            Assert.ArgumentNotNull(pane, nameof(pane));

            if (pane.Content is ISelectionTracker)
            {
                return;
            }

            IEnumerable<object> selection = null;

            var tracker = pane.Content as ISelectionTracking;
            if (tracker != null)
            {
                selection = tracker.GetSelectedObjects();
            }

            AppHost.Selection.Track(pane, selection);
        }

        public static void ShowExceptionMessageBox([NotNull] Exception exception)
        {
            Assert.ArgumentNotNull(exception, nameof(exception));

            var clipboard = string.Format(@"{0}{3}{1}{3}{3}Sitecore Rocks {2}", exception.Message, exception.StackTrace, Assembly.GetExecutingAssembly().GetFileVersion(), Environment.NewLine);

            var text = string.Format(Resources.SitecorePackage_HandleException_, clipboard);

            if (AppHost.MessageBox(text, Resources.SitecorePackage_HandleException_Sitecore_Unhandled_Exception, MessageBoxButton.OKCancel, MessageBoxImage.Error) != MessageBoxResult.OK)
            {
                return;
            }

            AppHost.Clipboard.SetText(clipboard);
            AppHost.Browsers.Navigate(@"http://sdn.sitecore.net/forum/ShowForum.aspx?ForumID=36");
        }

        public void WindowActivated([CanBeNull] EnvDTE.Window gotFocus, [CanBeNull] EnvDTE.Window lostFocus)
        {
            string got;
            string lost;

            try
            {
                got = gotFocus != null ? gotFocus.ObjectKind : string.Empty;
                lost = lostFocus != null ? lostFocus.ObjectKind : string.Empty;
            }
            catch
            {
                return;
            }

            if (got == lost)
            {
                return;
            }

            if (got == @"{CBDA4D53-D53A-4AD6-8069-2A680BC178B2}")
            {
                ActiveContext.Focused = Focused.ContentTree;
            }
            else if (got == @"{C918DE4A-FB6C-4BE2-8C84-C67278ACBBF8}")
            {
                ActiveContext.Focused = Focused.ContentEditor;

                // TODO: set content editor
            }
            else if (lost == @"{CBDA4D53-D53A-4AD6-8069-2A680BC178B2}")
            {
                ActiveContext.Focused = Focused.None;
            }
            else if (lost == @"{C918DE4A-FB6C-4BE2-8C84-C67278ACBBF8}")
            {
                ActiveContext.Focused = Focused.None;
            }
        }

        public void WindowClosing([NotNull] EnvDTE.Window window)
        {
            Assert.ArgumentNotNull(window, nameof(window));

            AppHost.Tasks.Clear("Sitecore Layouts", window.Caption);
        }

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			// Switches to the UI thread in order to consume some services used in command initialization
			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			AppHost.Container.Register<BrowserHost, VisualStudioBrowserHost>().AsSingleton();
            AppHost.Container.Register<FilesHost, VisualStudioFilesHost>().AsSingleton();
            AppHost.Container.Register<OutputHost, VisualStudioOutputHost>().AsSingleton();
            AppHost.Container.Register<ProjectHost, VisualStudioProjectHost>().AsSingleton();
            AppHost.Container.Register<SettingsHost, VisualStudioSettingsHost>().AsSingleton();
            AppHost.Container.Register<ShellHost, VisualStudioShellHost>().AsSingleton();
            AppHost.Container.Register<StatusbarHost, VisualStudioStatusbarHost>().AsSingleton();
            AppHost.Container.Register<TasksHost, VisualStudioTasksHost>().AsSingleton();
            AppHost.Container.Register<WindowsHost, VisualStudioWindowsHost>().AsSingleton();

            AppHost.Container.Register<VisualStudioWindowFactory, VisualStudioWindowFactory>().AsSingleton();

            AppHost.Shell.ShellIdentifier = Constants.SitecoreRocksVisualStudio;
            AppHost.User.UserFolderName = "Sitecore.Rocks.VisualStudio";

            AppHost.Usage.Report("Started Sitecore Rocks Visual Studio");

            AppHost.Shell.VisualStudioVersion = new Version(Dte.Version);
            AppHost.Shell.VisualStudioLocation = Path.Combine(GetProgramFilesFolder(), "Microsoft Visual Studio " + Dte.Version);

            Instance = this;

            AppHost.Plugins.ResolvePluginAssemblies += (fileNames, includePackages, includeAssemblies, includeServerComponents) => AppHost.Plugins.GetPluginAssemblies(fileNames, includePackages, includeAssemblies, includeServerComponents);
            AppHost.Plugins.UninstallHandlers += AppHost.Plugins.UninstallPlugins;

			Dispatcher.CurrentDispatcher.UnhandledException += HandleException;
            EventManager.RegisterClassHandler(typeof(System.Windows.Window), FrameworkElement.UnloadedEvent, new RoutedEventHandler(WindowUnloaded));

            AppHost.Shell.Initialize();

			var shellService = await GetServiceAsync(typeof(SVsShell)) as IVsShell;
            if (shellService != null)
            {
                ErrorHandler.ThrowOnFailure(shellService.AdviseShellPropertyChanges(this, out _eventSinkCookie));
            }
        }

        protected override int QueryClose(out bool canClose)
        {
            var result = base.QueryClose(out canClose);

            if (canClose)
            {
                AppHost.Usage.Report("Stopped Sitecore Rocks Visual Studio");
            }

            return result;
        }

        internal void RegisterEditor([NotNull] IVsEditorFactory factory)
        {
            Debug.ArgumentNotNull(factory, nameof(factory));

            RegisterEditorFactory(factory);
        }

        [NotNull]
        private string GetProgramFilesFolder()
        {
            if (IntPtr.Size == 8 || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? string.Empty;
            }

            return Environment.GetEnvironmentVariable("ProgramFiles") ?? string.Empty;
        }

        private void HandleException([NotNull] object sender, [NotNull] DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var friendlyException = e.Exception as FriendlyException;
            if (friendlyException != null)
            {
                e.Handled = friendlyException.Handle();
                return;
            }

            var shutDownException = e.Exception as InvalidOperationException;
            if (shutDownException != null && shutDownException.Message.Contains("The Application object is being shut down."))
            {
                return;
            }

            var stackTrace = e.Exception.StackTrace;
            if (stackTrace.IndexOf(@"Sitecore", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return;
            }

            if (stackTrace.IndexOf(@"Hedgehog", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return;
            }

            e.Handled = true;

            if (e.Exception is TypeLoadException)
            {
                if (AppHost.MessageBox(string.Format(Resources.SitecorePackage_HandleException_TypeLoadError, e.Exception.Message), Resources.Error, MessageBoxButton.OKCancel, MessageBoxImage.Error) != MessageBoxResult.OK)
                {
                    return;
                }

                var command = new ManagePlugins();
                AppHost.Usage.ReportCommand(command, this);
                command.Execute(this);
                return;
            }

            AppHost.Shell.HandleException(e.Exception);
        }

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, [NotNull] object propValue)
        {
            Debug.ArgumentNotNull(propValue, nameof(propValue));

            if ((int)__VSSPROPID.VSSPROPID_Zombie != propid)
            {
                return VSConstants.S_OK;
            }

            if ((bool)propValue)
            {
                return VSConstants.S_OK;
            }

            var shellService = GetService(typeof(SVsShell)) as IVsShell;
            if (shellService != null)
            {
                ErrorHandler.ThrowOnFailure(shellService.UnadviseShellPropertyChanges(_eventSinkCookie));
            }

            ShellLoadedPipeline.Run().WithParameters();

            _eventSinkCookie = 0;
            return VSConstants.S_OK;
        }

        private void WindowUnloaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            Notifications.RaiseUnloaded(sender, sender);
        }
    }
}
