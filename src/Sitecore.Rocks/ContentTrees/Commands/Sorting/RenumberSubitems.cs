// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    [Command(Submenu = "Sorting"), CommandId(CommandIds.SitecoreExplorer.RenumberSubitems, typeof(ContentTreeContext)), Feature(FeatureNames.Sorting)]
    public class RenumberSubitems : CommandBase
    {
        public RenumberSubitems()
        {
            Text = Resources.RenumberSubitems_RenumberSubitems_Renumber_SubItems;
            Group = "Renumber";
            SortingValue = 5100;
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

            var item = context.SelectedItems.First() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if ((item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
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

            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder");

            foreach (var i in context.SelectedItems)
            {
                var item = i as ItemTreeViewItem;
                if (item == null)
                {
                    continue;
                }

                var sortOrder = 0;

                foreach (var obj in item.Items)
                {
                    var child = obj as ItemTreeViewItem;
                    if (child == null)
                    {
                        continue;
                    }

                    ItemModifier.Edit(child.Item.ItemUri, fieldId, sortOrder.ToString());
                    child.Item.SortOrder = sortOrder;

                    sortOrder += 100;
                }
            }
        }
    }
}
