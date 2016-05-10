// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Repositories.Commands
{
    [Command]
    public class AddFile : CommandBase
    {
        public AddFile()
        {
            Text = "Add File...";
            Group = "File";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is IRepositorySelectionContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IRepositorySelectionContext;
            if (context == null)
            {
                return;
            }

            var repositoryList = RepositoryManager.GetRepository(context.RepositoryName);

            repositoryList.AddFile("Add File");

            context.RepositoryPanel.Refresh();
        }
    }
}
