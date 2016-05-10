// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Repositories.Commands
{
    [Command]
    public class ManageRepository : CommandBase
    {
        public ManageRepository()
        {
            Text = "Manage Repository...";
            Group = "Repository";
            SortingValue = 8000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IRepositorySelectionContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IRepositorySelectionContext;
            if (context == null)
            {
                return;
            }

            var repositoryList = RepositoryManager.GetRepository(context.RepositoryName);

            repositoryList.Edit("Manage Repository");

            context.RepositoryPanel.Refresh();
        }
    }
}
