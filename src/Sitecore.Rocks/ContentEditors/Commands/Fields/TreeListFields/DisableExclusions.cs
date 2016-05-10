// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.TreeListFields
{
    [Command]
    public class DisableExclusions : CommandBase
    {
        public DisableExclusions()
        {
            Text = "Disable Exclusions and Inclusions";
            Group = "TreeList";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;

            if (field.IsBlob)
            {
                return false;
            }

            var fieldControl = field.Control as TreeList;
            if (fieldControl == null)
            {
                return false;
            }

            IsChecked = fieldControl.DisableExclusions;

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var field = context.Field;

            var fieldControl = field.Control as TreeList;
            if (fieldControl == null)
            {
                return;
            }

            fieldControl.DisableExclusions = !fieldControl.DisableExclusions;

            foreach (var item in fieldControl.Source.Items.OfType<BaseTreeViewItem>())
            {
                item.Refresh();
            }
        }
    }
}
