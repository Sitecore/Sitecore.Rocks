// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell.Commands
{
    [Command, ShellMenuCommand(CommandIds.NewConnection)]
    public class NewConnection : CommandBase
    {
        public NewConnection()
        {
            Text = Resources.NewConnection_NewConnection_New_Connection;
            Group = "Connection";
            SortingValue = 50;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ShellContext;
        }

        public override void Execute(object parameter)
        {
            SiteManager.NewConnection();
        }
    }
}
