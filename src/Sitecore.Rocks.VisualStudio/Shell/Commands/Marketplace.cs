// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands
{
    [Command, ShellMenuCommand(CommandIds.Marketplace)]
    public class Marketplace : CommandBase
    {
        public Marketplace()
        {
            Text = "Sitecore Marketplace...";
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ShellContext;
        }

        public override void Execute(object parameter)
        {
            AppHost.Browsers.Navigate(@"http://marketplace.sitecore.net/SearchResults#query=rocks");
        }
    }
}
