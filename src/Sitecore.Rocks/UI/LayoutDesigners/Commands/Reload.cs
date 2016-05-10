// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands
{
    [Command, ToolbarElement(typeof(LayoutDesignerContext), 5900, "Layout", "Tasks", Icon = "Resources/32x32/Recurring.png")]
    public class Reload : CommandBase, IToolbarElement
    {
        public Reload()
        {
            Text = Resources.Reload;
            Group = "Reload";
            SortingValue = 9999;
            Icon = new Icon("Resources/16x16/refresh.png");
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is LayoutDesignerContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context != null)
            {
                context.LayoutDesigner.Reload();
            }
        }
    }
}
