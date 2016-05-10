// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command]
    public class AddField : CommandBase
    {
        public AddField()
        {
            Text = Resources.AddField_AddField_Add_Field;
            Group = "Fields";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            return false;
        }

        public override void Execute(object parameter)
        {
            var context = (TemplateDesignerContext)parameter;
            if (context == null)
            {
                return;
            }

            var section = context.Section;
            if (section == null)
            {
                return;
            }

            var templateDesigner = context.TemplateDesigner;

            var field = new TemplateDesigner.TemplateField();
            section.Fields.Add(field);

            field.Id = Guid.NewGuid().ToString("B").ToUpperInvariant();
            field.Section = section;
            field.Control = new TemplateField();
            field.Control.Initialize(templateDesigner, field);

            section.Control.FieldStack.Children.Add(field.Control);
        }
    }
}
