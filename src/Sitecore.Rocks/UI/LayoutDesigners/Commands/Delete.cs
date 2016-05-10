// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands
{
    [Command, ToolbarElement(typeof(LayoutDesignerContext), 1001, "Home", "Editing", Icon = "Resources/32x32/Cross.png")]
    public class Delete : CommandBase, IToolbarElement
    {
        public Delete()
        {
            Text = "Delete";
            Group = "Edit";
            SortingValue = 3000;
            Icon = new Icon("Resources/16x16/delete.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            return context.SelectedItems.Any();
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.SelectedItems.ToList();

            foreach (var renderingItem in selectedItems)
            {
                context.LayoutDesigner.LayoutDesignerView.RemoveRendering(renderingItem);
            }
        }
    }
}
