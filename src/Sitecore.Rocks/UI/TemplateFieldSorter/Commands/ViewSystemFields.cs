// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.TemplateFieldSorter.Commands
{
    [Command]
    public class ViewSystemFields : CommandBase
    {
        public ViewSystemFields()
        {
            Text = "System Fields";
            Group = "Fields";
            SortingValue = 5000;
        }

        public override bool CanExecute([CanBeNull] object parameter)
        {
            var context = parameter as TemplateFieldSorterContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = context.TemplateFieldSorter.SystemFields;

            return true;
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as TemplateFieldSorterContext;
            if (context == null)
            {
                return;
            }

            context.TemplateFieldSorter.SystemFields = !context.TemplateFieldSorter.SystemFields;
            context.TemplateFieldSorter.Refresh();
        }
    }
}
