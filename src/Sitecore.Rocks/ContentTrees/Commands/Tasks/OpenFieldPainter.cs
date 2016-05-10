// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Dialogs.FieldPainters;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), Feature(FeatureNames.AdvancedOperations)]
    public class OpenFieldPainter : CommandBase
    {
        public OpenFieldPainter()
        {
            Text = "Field Painter...";
            Group = "Fields";
            SortingValue = 4016;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            if (!context.Items.All(i => i.ItemUri.Site.DataService.CanExecuteAsync("Items.Save")))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var d = new FieldPainterDialog(item.ItemUri);
            AppHost.Shell.ShowDialog(d);
        }
    }
}
