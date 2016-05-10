// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Dialogs.SelectTemplatesDialogs;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command, ToolbarElement(typeof(TemplateDesignerContext), 1500, "Home", "Template", ElementType = RibbonElementType.LargeButton, Icon = "Resources/32x32/Gantt-Chart.png", Text = "Base Templates")]
    public class SetBaseTemplates : CommandBase, IToolbarElement
    {
        public SetBaseTemplates()
        {
            Text = Resources.SetBaseTemplates_SetBaseTemplates_Set_Base_Templates;
            Group = "Tasks";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.TemplateDesigner.BaseTemplates;

            var d = new SelectTemplatesDialog
            {
                HelpText = "Each data template inherits from zero or more base data templates, which in turn specify base templates.",
                Label = "Select the Base Templates:"
            };
            d.Initialize(Resources.SetBaseTemplates_Execute_Base_Templates, context.TemplateDesigner.TemplateUri.DatabaseUri, selectedItems);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            context.TemplateDesigner.BaseTemplates = d.SelectedTemplates;
            context.TemplateDesigner.SetModified(true);
        }
    }
}
