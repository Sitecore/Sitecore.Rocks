// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command]
    public class AddSection : CommandBase
    {
        public AddSection()
        {
            Text = Resources.AddSection_AddSection_Add_Section;
            Group = "Fields";
            SortingValue = 2000;
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

            var templateDesigner = context.TemplateDesigner;

            var section = new TemplateDesigner.TemplateSection();
            templateDesigner.Sections.Add(section);
            section.Id = Guid.NewGuid().ToString("B").ToUpperInvariant();

            section.Control = new TemplateSection();
            section.Control.Initialize(templateDesigner, section);

            templateDesigner.Stack.Children.Add(section.Control);
        }
    }
}
