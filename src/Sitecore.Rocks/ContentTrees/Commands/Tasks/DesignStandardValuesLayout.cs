// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.LayoutDesigners.Extensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), CommandId(CommandIds.ItemEditor.DesignStandardValuesLayout, typeof(ContentTreeContext))]
    public class DesignStandardValuesLayout : DesignLayoutBase
    {
        public DesignStandardValuesLayout()
        {
            Text = Resources.DesignStandardValuesLayout_DesignStandardValuesLayout_Design_Layout_on_Standard_Values;
            Group = "Items";
            SortingValue = 1050;
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

            var itemTreeViewItem = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (itemTreeViewItem == null)
            {
                return false;
            }

            var item = itemTreeViewItem.Item;

            if (item.StandardValuesId == ItemId.Empty)
            {
                return false;
            }

            if ((item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.EditLayouts) != DataServiceFeatureCapabilities.EditLayouts)
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

            var itemTreeViewItem = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (itemTreeViewItem == null)
            {
                return;
            }

            var item = itemTreeViewItem.Item;

            if (!CanDesign(item))
            {
                return;
            }

            var layoutUri = new ItemVersionUri(new ItemUri(item.ItemUri.DatabaseUri, item.StandardValuesId), LanguageManager.CurrentLanguage, Version.Latest);

            GetValueCompleted<Item> completed = i => AppHost.Env.LayoutDesigner().Open("PageDesigner" + i.Uri, i);

            item.ItemUri.Site.DataService.GetItemFieldsAsync(layoutUri, completed);
        }
    }
}
