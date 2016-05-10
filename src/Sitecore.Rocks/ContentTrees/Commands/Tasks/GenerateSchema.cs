// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.LayoutDesigners;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name)]
    public class GenerateSchema : CommandBase
    {
        private static readonly ItemId viewRenderingId = new ItemId(Constants.TemplateIds.ViewRenderingId);

        public GenerateSchema()
        {
            Text = "Update IntelliSense";
            Group = "Items";
            SortingValue = 1099;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var database = context.SelectedItems.FirstOrDefault() as DatabaseTreeViewItem;
            if (database != null)
            {
                if (!database.Site.IsSitecoreVersion(SitecoreVersion.Version70))
                {
                    return false;
                }
            }
            else
            {
                var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
                if (item == null)
                {
                    return false;
                }

                if (!item.ItemUri.Site.IsSitecoreVersion(SitecoreVersion.Version70))
                {
                    return false;
                }

                if (item.TemplateId != viewRenderingId)
                {
                    return false;
                }
            }

            if (string.IsNullOrEmpty(AppHost.Shell.VisualStudioLocation))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var databaseUri = DatabaseUri.Empty;

            var database = context.SelectedItems.FirstOrDefault() as DatabaseTreeViewItem;
            if (database != null)
            {
                databaseUri = database.DatabaseUri;
            }
            else
            {
                var selectedItem = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
                if (selectedItem != null)
                {
                    databaseUri = selectedItem.ItemUri.DatabaseUri;
                }
            }

            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            LayoutSchemaHelper.GenerateSchema(databaseUri, false, () => { });
        }
    }
}
