// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands
{
    [Command, ShellMenuCommand(CommandIds.VisitWebsite)]
    public class VisitWebsite : CommandBase
    {
        public VisitWebsite()
        {
            Text = "Visit the Sitecore Rocks web site";
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ShellContext;
        }

        public override void Execute(object parameter)
        {
            AppHost.Browsers.Navigate(@"http://vsplugins.sitecore.net");
        }
    }
}
