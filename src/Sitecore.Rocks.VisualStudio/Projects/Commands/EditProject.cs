// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data.DataServices.Dialogs;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;
using Sitecore.Rocks.UI.UpdateServerComponents;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.EditProject)]
    public class EditProject : SolutionCommand
    {
        public EditProject()
        {
            Text = Resources.EditProject_EditProject_Edit_Sitecore_Settings;
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

                var site = project.Site;
                if (site == null)
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

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            var siteEditor = new SiteEditor();

            siteEditor.Load(site);

            if (AppHost.Shell.ShowDialog(siteEditor) != true)
            {
                return;
            }

            ConnectionManager.Save();

            project.HostName = site.HostName;
            project.UserName = site.UserName;
            project.Save();

            Notifications.RaiseSiteChanged(this, site);

            var editor = siteEditor.DataServiceEditor as WebServiceSiteEditor;
            if (editor != null)
            {
                if (editor.AutomaticUpdate.IsChecked == true)
                {
                    UpdateServerComponentsDialog.AutomaticUpdate(site.DataService, site.Name, site.WebRootPath, site);
                }
            }
        }
    }
}
