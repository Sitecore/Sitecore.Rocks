// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command, ToolbarElement(typeof(TemplateDesignerContext), 1520, "Home", "Template", ElementType = RibbonElementType.LargeButton, Text = "Set Icon", Icon = "Resources/32x32/Photo.png")]
    public class SetIcon : CommandBase, IToolbarElement
    {
        public SetIcon()
        {
            Text = Resources.SetIcon_SetIcon_Set_Icon;
            Group = "Tasks";
            SortingValue = 200;
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

            var d = new SetIconDialog();
            d.Initialize(context.TemplateDesigner.TemplateUri.Site, context.TemplateDesigner.TemplateIcon);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            context.TemplateDesigner.TemplateIcon = d.FileName;

            var path = @"/sitecore/shell/~/icon/" + d.FileName.Replace(@"16x16", @"32x32");

            var icon = new Icon(context.TemplateDesigner.TemplateUri.Site, path);

            context.TemplateDesigner.QuickInfoIcon.Source = icon.GetSource();

            context.TemplateDesigner.SetModified(true);
        }
    }
}
