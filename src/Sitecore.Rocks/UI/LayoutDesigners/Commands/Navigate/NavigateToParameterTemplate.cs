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
    [Command(Submenu = NavigateSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4420, "Rendering", "Navigate", Text = "Parameters Template", ElementType = RibbonElementType.SmallButton)]
    public class NavigateToParameterTemplate : CommandBase, IToolbarElement
    {
        public NavigateToParameterTemplate()
        {
            Text = "Locate Parameters Template in Sitecore Explorer";
            Group = "Renderings";
            SortingValue = 1000;
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

            var item = context.SelectedItem as RenderingItem;
            if (item == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(item.ParameterTemplateId);
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

            var item = context.SelectedItem as RenderingItem;
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
                    AppHost.Windows.OpenTemplateDesigner(itemHeader.ItemUri);
                }
                else
                {
                    contentTree.Locate(itemHeader.ItemUri);
                }
            };

            databaseUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, item.ParameterTemplateId, databaseUri.DatabaseName.ToString());
        }
    }
}
