// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects.Dialogs;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.Revert)]
    public class Revert : SolutionCommand
    {
        public Revert()
        {
            Text = Resources.Revert_Revert_Revert___;
        }

        public override bool CanExecute(object parameter)
        {
            IsVisible = false;

            if (!(parameter is ShellContext))
            {
                return false;
            }

            var selectedItems = GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                return false;
            }

            var items = GetProjectItems(selectedItems);

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            if (AnyItem(items, item => ProjectManager.GetProject(item) == null))
            {
                return false;
            }

            if (AnyProjectItem(items, IsLocalSitecore))
            {
                return false;
            }

            if (!AnyProjectItem(items, IsRevertable))
            {
                return false;
            }

            IsVisible = true;
            return true;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count == 0)
            {
                return;
            }

            var items = GetProjectItems(selectedItems);

            var w = new RevertWindow();

            w.Initialize(items);

            AppHost.Shell.ShowDialog(w);
        }

        private bool IsLocalSitecore([NotNull] ProjectItem projectItem)
        {
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));

            return !projectItem.Project.IsRemoteSitecore;
        }

        private bool IsRevertable([NotNull] ProjectItem projectItem)
        {
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));

            return !projectItem.IsAdded;
        }
    }
}
