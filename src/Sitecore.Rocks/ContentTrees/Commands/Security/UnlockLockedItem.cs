// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Security
{
    [Command, CommandId(CommandIds.SitecoreExplorer.UnlockLockedItem, typeof(ContentTreeContext)), Feature(FeatureNames.Security)]
    public class UnlockLockedItem : CommandBase
    {
        public UnlockLockedItem()
        {
            Text = Resources.Unlock_Unlock_Unlock_Item;
            Group = "Security";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            foreach (var item in context.Items)
            {
                var itemData = item as IItemData;
                if (itemData == null)
                {
                    return false;
                }

                if (itemData.GetData("locked.item") != "true")
                {
                    return false;
                }

                if ((item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
                {
                    return false;
                }
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                DataService.HandleExecute(response, result);

                var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Workflow/Workflow/__Lock");

                foreach (var i in context.Items)
                {
                    var fieldUri = new FieldUri(new ItemVersionUri(i.ItemUri, LanguageManager.CurrentLanguage, Version.Latest), fieldId);
                    Notifications.RaiseFieldChanged(this, fieldUri, string.Empty);
                }

                var item = context.Items.FirstOrDefault() as BaseTreeViewItem;
                if (item == null)
                {
                    return;
                }

                var parent = item.GetParentTreeViewItem() as BaseTreeViewItem;
                if (parent == null)
                {
                    return;
                }

                parent.Refresh();
            };

            var itemList = new StringBuilder();
            IItem first = null;

            foreach (var item in context.Items)
            {
                if (first == null)
                {
                    first = item;
                }
                else
                {
                    itemList.Append("|");
                }

                itemList.Append(item.ItemUri.ItemId);
            }

            if (first != null)
            {
                first.ItemUri.Site.DataService.ExecuteAsync("Security.Unlock", completed, first.ItemUri.DatabaseName.Name, itemList.ToString());
            }
        }
    }
}
