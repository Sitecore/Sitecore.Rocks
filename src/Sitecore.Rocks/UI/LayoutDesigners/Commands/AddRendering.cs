// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands
{
    [ToolbarElement(typeof(LayoutDesignerContext), 3000, "Home", "Renderings", Icon = "Resources/32x32/Plus.png")]
    public class AddRendering : CommandBase, IToolbarElement
    {
        public AddRendering()
        {
            Text = "Add Rendering";
            Group = "Edit";
            SortingValue = 990;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is LayoutDesignerContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            context.LayoutDesigner.AddRendering();
        }
    }
}
