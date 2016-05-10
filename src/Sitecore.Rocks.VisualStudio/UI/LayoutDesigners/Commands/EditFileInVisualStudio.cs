// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands
{
    [Command]
    public class EditFileInVisualStudio : CommandBase
    {
        public EditFileInVisualStudio()
        {
            Text = "Edit Rendering File in Visual Studio Project";
            Group = "Renderings";
            SortingValue = 150;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                return false;
            }

            var item = selectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return false;
            }

            if (SitecorePackage.Instance.Dte.Solution == null)
            {
                return false;
            }

            return ProjectManager.GetProjectFileItem(item.ItemUri) != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                return;
            }

            var itemTreeViewItem = selectedItems.FirstOrDefault() as RenderingItem;
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
