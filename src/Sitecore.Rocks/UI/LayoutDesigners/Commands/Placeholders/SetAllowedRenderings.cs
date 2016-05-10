// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.Dialogs.SelectRenderingsDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Placeholders
{
    [Command]
    public class SetAllowedRenderings : CommandBase, IToolbarElement
    {
        private static readonly char[] Pipe =
        {
            '|'
        };

        public SetAllowedRenderings()
        {
            Text = "Set Allowed Renderings...";
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

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
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

            if (!string.IsNullOrEmpty(item.MetaDataItemId))
            {
                GetValueCompleted<Item> completed = delegate(Item placeholderSettingsItem)
                {
                    var selectedItems = new List<ItemId>();

                    var allowedControls = placeholderSettingsItem.Fields.FirstOrDefault(f => f.Name == "Allowed Controls");
                    if (allowedControls != null)
                    {
                        selectedItems.AddRange(allowedControls.Value.Split(Pipe, StringSplitOptions.RemoveEmptyEntries).Where(v => !string.IsNullOrWhiteSpace(v)).Select(id => new ItemId(new Guid(id))));
                    }

                    SetAllowed(item, selectedItems);
                };

                var itemVersionUri = new ItemVersionUri(new ItemUri(item.DatabaseUri, new ItemId(new Guid(item.MetaDataItemId))), Language.Current, Data.Version.Latest);
                item.DatabaseUri.Site.DataService.GetItemFieldsAsync(itemVersionUri, completed);

                return;
            }

            var d = new SelectItemDialog();
            d.Initialize("Create Placeholder Settings Item", item.DatabaseUri, "/sitecore/layout/Placeholder Settings");
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var templateId = IdManager.GetItemId("/sitecore/templates/System/Layout/Placeholder");
            var templateUri = new ItemUri(item.DatabaseUri, templateId);

            var newItemUri = AppHost.Server.AddFromTemplate(d.SelectedItemUri, templateUri, item.Name + "Settings");
            if (newItemUri == ItemUri.Empty)
            {
                return;
            }

            item.MetaDataItemId = newItemUri.ItemId.ToString();
            SetAllowed(item, Enumerable.Empty<ItemId>());
        }

        private void SetAllowed([NotNull] PlaceholderItem item, IEnumerable<ItemId> selectedItems)
        {
            var dialog = new SelectRenderingsDialog();
            dialog.Initialize("Set Allowed Placeholders", item.DatabaseUri, selectedItems);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var itemUri = new ItemUri(item.DatabaseUri, new ItemId(new Guid(item.MetaDataItemId)));

            ItemModifier.Edit(itemUri, "{E391B526-D0C5-439D-803E-17512EAE6222}", string.Join("|", dialog.SelectedRenderings));
        }
    }
}
