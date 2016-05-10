// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Packages.PluginManagers;

namespace Sitecore.Rocks.Shell.Commands
{
    [Command, ShellMenuCommand(CommandIds.ManagePlugins)]
    public class ManagePlugins : CommandBase
    {
        public ManagePlugins()
        {
            Text = "Manage Plugins...";
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ShellContext;
        }

        public override void Execute(object parameter)
        {
            var dialog = new PluginManagerDialog();
            AppHost.Shell.ShowDialog(dialog);
        }
    }
}
