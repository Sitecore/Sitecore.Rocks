// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command, ToolbarElement(typeof(TemplateDesignerContext), 1540, "Home", "Fields", ElementType = RibbonElementType.SmallButton)]
    public class DeleteSection : CommandBase, IToolbarElement
    {
        public DeleteSection()
        {
            Text = Resources.DeleteSection_DeleteSection_Delete_Section;
            Group = "Sections";
            SortingValue = 100;
            Icon = new Icon("Resources/16x16/delete.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return false;
            }

            var templateSection = context.Section;
            if (templateSection == null)
            {
                return false;
            }

            return !templateSection.Control.IsLastSection();
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return;
            }

            var templateSection = context.Section;
            if (templateSection == null)
            {
                return;
            }

            for (var i = templateSection.Fields.Count - 1; i >= 0; i--)
            {
                var field = templateSection.Fields[i];
                field.Section.Fields.Remove(field);
            }

            context.TemplateDesigner.Sections.Remove(templateSection);
            context.TemplateDesigner.Stack.Children.Remove(templateSection.Control);
            context.TemplateDesigner.SetModified(true);
        }
    }
}
