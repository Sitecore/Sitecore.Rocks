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
    [Command(Submenu = "Security"), RuleAction("lock item", "Security"), CommandId(CommandIds.SitecoreExplorer.Lock, typeof(ContentTreeContext)), Feature(FeatureNames.Security)]
    public class Lock : CommandBase, IRuleAction
    {
        public Lock()
        {
            Text = Resources.Lock_Lock_Lock;
            Group = "Locking";
            SortingValue = 1100;
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

                if (!string.IsNullOrEmpty(itemHeader.Item.Locked))
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

                    itemHeader.Item.Locked = item.ItemUri.Site.Credentials.UserName;

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
                    itemList.Append("|");
                }

                itemList.Append(item.ItemUri.ItemId);
            }

            if (first != null)
            {
                first.ItemUri.Site.DataService.ExecuteAsync("Security.Lock", completed, first.ItemUri.DatabaseName.Name, itemList.ToString());
            }
        }
    }
}
