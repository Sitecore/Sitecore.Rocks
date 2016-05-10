// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.TemplateDesigner;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Templates
{
    [Command(Submenu = TasksSubmenu.Name), ToolbarElement(typeof(TemplateDesignerContext), 1525, "Home", "Template", ElementType = RibbonElementType.LargeButton, Icon = "Resources/32x32/Sort-Ascending.png", Text = "Sort Fields")]
    public class SortTemplateFields : CommandBase, IToolbarElement
    {
        public SortTemplateFields()
        {
            Text = "Sort Template Fields";
            Group = "Templates";
            SortingValue = 5990;
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

            var item = context.Items.First() as ITemplatedItem;
            return item != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.First() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            if (IdManager.IsTemplate(item.TemplateId, "template"))
            {
                AppHost.Windows.Factory.OpenTemplateFieldSorter(item.ItemUri);
                return;
            }

            var templateUri = new ItemUri(item.ItemUri.DatabaseUri, item.TemplateId);
            AppHost.Windows.Factory.OpenTemplateFieldSorter(templateUri);
        }
    }
}
