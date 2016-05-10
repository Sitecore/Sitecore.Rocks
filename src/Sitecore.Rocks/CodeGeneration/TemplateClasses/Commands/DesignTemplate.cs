// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.CodeGeneration.TemplateClasses.Commands
{
    [Command]
    public class DesignTemplate : CommandBase
    {
        public DesignTemplate()
        {
            Text = "Design Template";
            Group = "Design";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TemplateClassesContext;
            if (context == null)
            {
                return false;
            }

            if (!context.DesignSurface.SelectedItems.Any())
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateClassesContext;
            if (context == null)
            {
                return;
            }

            foreach (var selectedItem in context.DesignSurface.SelectedItems)
            {
                var content = selectedItem.ShapeContent as TemplateShapeContent;
                if (content == null)
                {
                    continue;
                }

                AppHost.Windows.Factory.OpenTemplateDesigner(content.Template.TemplateUri);
            }
        }
    }
}
