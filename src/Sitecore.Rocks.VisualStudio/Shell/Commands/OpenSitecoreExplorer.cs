// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Shell.Commands
{
    [Command, ShellMenuCommand(CommandIds.OpenSitecoreExplorer)]
    public class OpenSitecoreExplorer : CommandBase
    {
        public OpenSitecoreExplorer()
        {
            Text = Resources.OpenSitecoreExplorer_OpenSitecoreExplorer_Sitecore_Explorer;
            Icon = new Icon("Resources/16x16/text_tree.png");
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ShellContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ShellContext;
            if (context == null)
            {
                return;
            }

            AppHost.Windows.Factory.OpenContentTree();
        }
    }
}
