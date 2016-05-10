// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command, ToolbarElement(typeof(TemplateDesignerContext), 1530, "Home", "Fields", ElementType = RibbonElementType.SmallButton)]
    public class DeleteField : CommandBase, IToolbarElement
    {
        public DeleteField()
        {
            Text = Resources.DeleteField_DeleteField_Delete_Field;
            Group = "Fields";
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

            var templateField = context.Field;
            if (templateField == null)
            {
                return false;
            }

            return !templateField.Control.IsLastField();
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return;
            }

            var templateField = context.Field;
            if (templateField == null)
            {
                return;
            }

            templateField.Section.Fields.Remove(context.Field);
            templateField.Section.Control.FieldStack.Children.Remove(templateField.Control);
            context.TemplateDesigner.SetModified(true);
        }
    }
}
