// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects.Dialogs;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.CommitToSitecore)]
    public class Commit : SolutionCommand
    {
        public Commit()
        {
            Text = Resources.Commit_Commit_Commit___;
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

            if (AnyItem(items, WithoutProject))
            {
                return false;
            }

            if (AnyProjectItem(items, IsLocalSitecore))
            {
                return false;
            }

            if (!AnyProjectItem(items, IsCommittable))
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

            var dialog = new CommitWindow();

            dialog.Initialize(items);

            AppHost.Shell.ShowDialog(dialog);
        }

        private bool IsCommittable([NotNull] ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            return projectItem.IsModified || projectItem.IsAdded || projectItem.IsConflict || !projectItem.IsValid;
        }

        private bool IsLocalSitecore([NotNull] ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            return !projectItem.Project.IsRemoteSitecore;
        }

        private bool WithoutProject([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return true;
            }

            var site = project.Site;
            if (site == null)
            {
                return true;
            }

            if ((site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Projects) != DataServiceFeatureCapabilities.Projects)
            {
                return true;
            }

            return false;
        }
    }
}
