// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.About;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.LearnAboutSitecoreRocks;

namespace Sitecore.Rocks.Shell.Commands
{
    [Command, ShellMenuCommand(CommandIds.About), StartPageCommand("Open the About box", StartPageLearnAboutSitecoreRocksGroup.Name, 9000)]
    public class About : CommandBase, IStartPageCommand
    {
        public About()
        {
            Text = Resources.About_About_About___;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ShellContext;
        }

        public override void Execute(object parameter)
        {
            Execute();
        }

        bool IStartPageCommand.CanExecute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            return true;
        }

        void IStartPageCommand.Execute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            Execute();
        }

        private void Execute()
        {
            var d = new AboutBox();
            AppHost.Shell.ShowDialog(d);
        }
    }
}
