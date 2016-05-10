// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.LayoutDesigners.Extensions;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), ToolbarElement(typeof(IItemSelectionContext), 3000, "Layout", "Design", Icon = "Resources/32x32/Window-Edit.png")]
    public class DesignLayout : DesignLayoutBase, IToolbarElement
    {
        public DesignLayout()
        {
            Text = Resources.Design_Layout;
            Group = "Items";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            return (item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.EditTemplate) == DataServiceFeatureCapabilities.EditTemplate;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.First();
            if (item == null)
            {
                return;
            }

            if (!CanDesign(item))
            {
                return;
            }

            var itemUri = new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest);

            GetValueCompleted<Item> completed = i => AppHost.Env.LayoutDesigner().Open("PageDesigner" + itemUri, i);

            itemUri.Site.DataService.GetItemFieldsAsync(itemUri, completed);
        }
    }
}
