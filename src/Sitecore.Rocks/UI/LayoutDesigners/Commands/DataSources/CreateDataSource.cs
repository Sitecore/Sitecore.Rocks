// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.DataSources
{
    [Command(Submenu = DataSourceSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4220, "Rendering", "Data Sources", Icon = "Resources/16x16/Document-Add.png", ElementType = RibbonElementType.SmallButton)]
    public class CreateDataSource : CommandBase, IToolbarElement
    {
        public CreateDataSource()
        {
            Text = "Create";
            Group = "DataSource";
            SortingValue = 1100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(item.DataSource))
            {
                return false;
            }

            return !string.IsNullOrEmpty(item.DataSourceTemplate);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var renderingTreeViewItem = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (renderingTreeViewItem == null)
            {
                return;
            }

            var itemUri = context.LayoutDesigner.FieldUris.First().ItemVersionUri.ItemUri;
            var databaseUri = context.LayoutDesigner.DatabaseUri;

            string name;
            renderingTreeViewItem.ParameterDictionary.Parameters.TryGetValue("Id", out name);
            if (string.IsNullOrEmpty(name))
            {
                name = renderingTreeViewItem.Name;
            }

            var location = renderingTreeViewItem.DataSourceLocation;
            if (string.IsNullOrEmpty(location))
            {
                var dialog = new SelectItemDialog();
                dialog.Initialize("Create Data Source", itemUri);
                if (AppHost.Shell.ShowDialog(dialog) != true)
                {
                    return;
                }
            }

            var template = renderingTreeViewItem.DataSourceTemplate;

            Site.RequestCompleted completed = delegate(string response)
            {
                var parts = response.Split(',');
                if (parts.Length != 2)
                {
                    AppHost.MessageBox("The item was not found.", Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var itemId = parts[0];
                var parentItemId = parts[1];

                Guid guid;
                if (!Guid.TryParse(itemId, out guid))
                {
                    AppHost.MessageBox("The item was not found.", Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                renderingTreeViewItem.DataSource = itemId;

                var itemVersionUri = new ItemVersionUri(new ItemUri(databaseUri, new ItemId(guid)), LanguageManager.CurrentLanguage, Data.Version.Latest);

                Notifications.RaiseItemAdded(this, itemVersionUri, new ItemUri(databaseUri, new ItemId(new Guid(parentItemId))));

                AppHost.OpenContentEditor(itemVersionUri);
            };

            databaseUri.Site.Execute("LayoutBuilders.CreateDataSource", completed, databaseUri.DatabaseName.ToString(), itemUri.ItemId.ToString(), name, location, template);
        }
    }
}
