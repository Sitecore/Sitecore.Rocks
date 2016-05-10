// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Input;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Navigate
{
    [Command(Submenu = NavigateSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4410, "Rendering", "Navigate", Text = "Placeholder", ElementType = RibbonElementType.SmallButton)]
    public class NavigateToPlaceholderSettings : CommandBase, IToolbarElement
    {
        public NavigateToPlaceholderSettings()
        {
            Text = "Locate Placeholder Settings in Sitecore Explorer";
            Group = "PlaceHolders";
            SortingValue = 200;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            var tree = ActiveContext.ActiveContentTree;
            if (tree == null)
            {
                return false;
            }

            var selectedItem = context.SelectedItem;
            if (selectedItem == null)
            {
                return false;
            }

            var item = selectedItem as PlaceholderItem;
            if (item == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(item.MetaDataItemId);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree == null)
            {
                return;
            }

            var selectedItem = context.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }

            var item = selectedItem as PlaceholderItem;
            if (item == null)
            {
                return;
            }

            var databaseUri = context.LayoutDesigner.DatabaseUri;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(databaseUri, element);

                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    AppHost.OpenContentEditor(itemHeader.ItemUri);
                }
                else
                {
                    contentTree.Locate(itemHeader.ItemUri);
                }
            };

            databaseUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, item.MetaDataItemId, databaseUri.DatabaseName.ToString());
        }
    }
}
