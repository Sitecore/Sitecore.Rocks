// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command, ToolbarElement(typeof(TemplateDesignerContext), 2000, "Home", "Tasks", ElementType = RibbonElementType.LargeButton, Icon = "Resources/32x32/Recurring.png")]
    public class Reload : CommandBase, IToolbarElement
    {
        public Reload()
        {
            Text = Resources.Reload;
            Group = "Reload";
            SortingValue = 9900;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is TemplateDesignerContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return;
            }

            context.TemplateDesigner.Initialize(context.TemplateDesigner.TemplateUri);
        }
    }
}
