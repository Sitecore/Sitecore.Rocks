// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.StartPage;

namespace Sitecore.Rocks.Shell.Commands
{
    [Command, ShellMenuCommand(CommandIds.OpenStartPage)]
    public class OpenStartPage : CommandBase
    {
        public OpenStartPage()
        {
            Text = "Start Page";
            Icon = new Icon("Resources/16x16/window_next.png");
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

            StartPageViewer.Open();
        }
    }
}
