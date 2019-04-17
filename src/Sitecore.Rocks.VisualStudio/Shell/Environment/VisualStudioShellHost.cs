// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioShellHost : ShellHost
    {
        public override string CommandLineArguments
        {
            get { return SitecorePackage.Instance.Dte.CommandLineArguments; }
        }

        public override MessageBoxResult MessageBox(string text, string title, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxIcon)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(title, nameof(title));

            var button = OLEMSGBUTTON.OLEMSGBUTTON_OK;
            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    button = OLEMSGBUTTON.OLEMSGBUTTON_OK;
                    break;
                case MessageBoxButton.OKCancel:
                    button = OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL;
                    break;
                case MessageBoxButton.YesNoCancel:
                    button = OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL;
                    break;
                case MessageBoxButton.YesNo:
                    button = OLEMSGBUTTON.OLEMSGBUTTON_YESNO;
                    break;
            }

            var icon = OLEMSGICON.OLEMSGICON_NOICON;
            switch (messageBoxIcon)
            {
                case MessageBoxImage.Error:
                    icon = OLEMSGICON.OLEMSGICON_CRITICAL;
                    break;
                case MessageBoxImage.Information:
                    icon = OLEMSGICON.OLEMSGICON_INFO;
                    break;
                case MessageBoxImage.Question:
                    icon = OLEMSGICON.OLEMSGICON_QUERY;
                    break;
                case MessageBoxImage.None:
                    icon = OLEMSGICON.OLEMSGICON_NOICON;
                    break;
                case MessageBoxImage.Warning:
                    icon = OLEMSGICON.OLEMSGICON_WARNING;
                    break;
            }

			var pressed = 0;
			ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				pressed = VsShellUtilities.ShowMessageBox(SitecorePackage.Instance, text, string.Empty, icon, button, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
			});

            var result = MessageBoxResult.None;
            switch (pressed)
            {
                case 1:
                    result = MessageBoxResult.OK;
                    break;
                case 2:
                    result = MessageBoxResult.Cancel;
                    break;
                case 3:
                    result = MessageBoxResult.Cancel;
                    break;
                case 4:
                    result = MessageBoxResult.None;
                    break;
                case 5:
                    result = MessageBoxResult.None;
                    break;
                case 6:
                    result = MessageBoxResult.Yes;
                    break;
                case 7:
                    result = MessageBoxResult.No;
                    break;
            }

            return result;
        }

        public override void SaveActiveDocument()
        {
            var activeDocument = SitecorePackage.Instance.Dte.ActiveDocument;
            if (activeDocument != null)
            {
                activeDocument.Save();
            }
        }

        public override bool? ShowDialog(Window dialog)
        {
            Assert.ArgumentNotNull(dialog, nameof(dialog));

			IVsUIShell shell = null;
			ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				shell = SitecorePackage.Instance.GetService<IVsUIShell>();
			});
			
            if (shell == null)
            {
                AppHost.Output.Log("Failed to get IVsUIShell");
                return base.ShowDialog(dialog);
            }

            shell.EnableModeless(0);
            try
            {
                return base.ShowDialog(dialog);
            }
            finally
            {
                shell.EnableModeless(1);
            }
        }
    }
}
