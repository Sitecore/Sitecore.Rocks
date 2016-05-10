// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command]
    public class ShowRibbon : CommandBase
    {
        public ShowRibbon()
        {
            Text = "Show Ribbon";
            Group = "View";
            SortingValue = 9000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = AppHost.Settings.GetBool("TemplateDesigner", "ShowRibbon", false);
            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateDesignerContext;
            if (context == null)
            {
                return;
            }

            AppHost.Settings.SetBool("TemplateDesigner", "ShowRibbon", !AppHost.Settings.GetBool("TemplateDesigner", "ShowRibbon", false));
            context.TemplateDesigner.ShowRibbon();
        }
    }
}
