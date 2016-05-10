// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    [Command(Submenu = "Sorting"), CommandId(CommandIds.SitecoreExplorer.ResetSortOrder, typeof(ContentTreeContext)), Feature(FeatureNames.Sorting)]
    public class ResetSortOrder : CommandBase
    {
        public ResetSortOrder()
        {
            Text = Resources.ResetSortOrder_ResetSortOrder_Reset_Sort_Order;
            Group = "Reset";
            SortingValue = 5000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (!context.SelectedItems.Any())
            {
                return false;
            }

            if (!context.SelectedItems.All(item => item is ItemTreeViewItem))
            {
                return false;
            }

            if (context.SelectedItems.Any(item => (((ItemTreeViewItem)item).Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute))
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

            foreach (var i in context.SelectedItems)
            {
                var item = i as ItemTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
                {
                    if (!DataService.HandleExecute(response, executeResult))
                    {
                        return;
                    }

                    var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder");
                    var fieldUri = new FieldUri(new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest), fieldId);
                    Notifications.RaiseFieldChanged(this, fieldUri, string.Empty);

                    item.RefreshItem();
                };

                item.ItemUri.Site.DataService.ExecuteAsync("ResetSortOrder", completed, item.ItemUri.ItemId.ToString(), item.ItemUri.DatabaseName.Name);
            }
        }
    }
}
