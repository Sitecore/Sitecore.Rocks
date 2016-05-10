// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.EditProjectFile)]
    public class EditProjectFile : SolutionCommand
    {
        public override bool CanExecute(object parameter)
        {
            IsVisible = false;

            if (!(parameter is ShellContext))
            {
                return false;
            }

            var selectedItems = GetSelectedItems();
            if (selectedItems.Count != 1)
            {
                return false;
            }

            var items = GetProjects(selectedItems);

            if (!items.All(i => i.IsValidProjectKind()))
            {
                return false;
            }

            foreach (var item in items)
            {
                var project = ProjectManager.GetProject(item.FileName);
                if (project == null)
                {
                    return false;
                }
            }

            IsVisible = true;
            return true;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count != 1)
            {
                return;
            }

            var items = GetProjects(selectedItems);
            var item = items[0];

            var project = ProjectManager.GetProject(item.FileName);
            if (project == null)
            {
                return;
            }

            SitecorePackage.Instance.Dte.Application.ItemOperations.OpenFile(project.FileName);
        }
    }
}
