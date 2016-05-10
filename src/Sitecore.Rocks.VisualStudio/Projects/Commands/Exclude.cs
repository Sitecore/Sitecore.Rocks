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
    [Command, ShellMenuCommand(CommandIds.Exclude)]
    public class Exclude : SolutionCommand
    {
        public Exclude()
        {
            Text = Resources.Exclude_Exclude_Exclude;
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

            if (AnyItem(items, item => ProjectManager.GetProject(item) == null))
            {
                return false;
            }

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            if (!AnyProjectItem(items, IsExcludable))
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

            var w = new ExcludeWindow();

            w.Initialize(items);

            AppHost.Shell.ShowDialog(w);
        }

        private bool IsExcludable([NotNull] ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return true;
        }
    }
}
