// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.RemoveProject)]
    public class RemoveProject : SolutionCommand
    {
        public RemoveProject()
        {
            Text = Resources.RemoveProject_RemoveProject_Remove_Sitecore_Project;
        }

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
            if (items.Count != 1)
            {
                return false;
            }

            if (!items.All(i => i.IsValidProjectKind()))
            {
                return false;
            }

            var item = items[0];

            var result = ProjectManager.GetProject(item.FileName) != null;

            IsVisible = result;
            return result;
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

            if (AppHost.MessageBox(Resources.RemoveProject_Execute_Are_you_sure_you_want_to_disconnect_from_Sitecore_, Resources.RemoveProject_Execute_Disconnect, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            project.Delete();

            ProjectManager.UnloadProject(project);
        }
    }
}
