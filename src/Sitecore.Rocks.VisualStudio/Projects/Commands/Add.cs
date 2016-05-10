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
    [Command, ShellMenuCommand(CommandIds.AddToSitecore)]
    public class Add : SolutionCommand
    {
        public Add()
        {
            Text = Resources.Add_Add_Add_to_Sitecore;
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

            if (!AnyItem(items, IsExcluded))
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

            var dialog = new AddWindow();
            dialog.Initialize(items);
            AppHost.Shell.ShowDialog(dialog);
        }

        private bool IsExcluded([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            return project.GetProjectItem(item) == null;
        }
    }
}
