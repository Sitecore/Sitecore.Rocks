// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.TemplateFieldSorter.Commands
{
    [Command]
    public class UpdateView : CommandBase
    {
        public UpdateView()
        {
            Text = "Update View";
            Group = "View";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TemplateFieldSorterContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as TemplateFieldSorterContext;
            if (context == null)
            {
                return;
            }

            context.TemplateFieldSorter.Refresh();
        }
    }
}
