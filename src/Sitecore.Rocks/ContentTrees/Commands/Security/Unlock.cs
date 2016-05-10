// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Security
{
    [Command(Submenu = "Security"), RuleAction("unlock item", "Security"), CommandId(CommandIds.SitecoreExplorer.Unlock, typeof(ContentTreeContext)), Feature(FeatureNames.Security)]
    public class Unlock : CommandBase, IRuleAction
    {
        public Unlock()
        {
            Text = Resources.Unlock_Unlock_Unlock;
            Group = "Locking";
            SortingValue = 1000;
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
                var itemHeader = item as ItemTreeViewItem;
                if (itemHeader == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(itemHeader.Item.Locked))
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
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Workflow/Workflow/__Lock");

                foreach (var item in context.Items)
                {
                    var itemHeader = item as ItemTreeViewItem;
                    if (itemHeader == null)
                    {
                        continue;
                    }

                    itemHeader.Item.Locked = string.Empty;
                    var fieldUri = new FieldUri(new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest), fieldId);
                    Notifications.RaiseFieldChanged(this, fieldUri, string.Empty);
                }
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
                    itemList.Append('|');
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
