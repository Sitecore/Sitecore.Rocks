// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command]
    public class EditFile : CommandBase
    {
        public EditFile()
        {
            Text = Resources.EditFile_EditFile_Edit_File;
            Group = "Layout";
            SortingValue = 5;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.SelectedItems.ToList();
            if (selectedItems.Count() != 1)
            {
                return false;
            }

            var item = selectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            return ProjectManager.GetProjectFileItem(item.ItemUri) != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.SelectedItems.ToList();
            if (selectedItems.Count() != 1)
            {
                return;
            }

            var itemTreeViewItem = selectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (itemTreeViewItem == null)
            {
                return;
            }

            var projectFile = ProjectManager.GetProjectFileItem(itemTreeViewItem.ItemUri);
            if (projectFile == null)
            {
                return;
            }

            var dte = SitecorePackage.Instance.Dte;

            var item = dte.Solution.FindProjectItem(projectFile.Path);
            if (item == null)
            {
                return;
            }

            var window = item.Open();
            if (window != null)
            {
                window.Visible = true;
            }
        }
    }
}
