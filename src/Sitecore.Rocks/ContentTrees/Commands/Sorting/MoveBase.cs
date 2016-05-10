// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.UI.KeyboardSchemes;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    public abstract class MoveBase : CommandBase
    {
        protected MoveBase()
        {
            Group = "Move";
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if ((item.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            var sortOrder = GetSortOrder(item);

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder");
                var fieldUri = new FieldUri(new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest), fieldId);
                Notifications.RaiseFieldChanged(this, fieldUri, sortOrder.ToString());

                var parent = item.GetParentTreeViewItem() as BaseTreeViewItem;
                if (parent == null)
                {
                    return;
                }

                KeyboardManager.IsActive++;
                try
                {
                    var itemTreeViewItem = parent as ItemTreeViewItem;
                    if (itemTreeViewItem != null)
                    {
                        itemTreeViewItem.RefreshPreservingSelection();
                    }
                    else
                    {
                        parent.Refresh();
                    }
                }
                finally
                {
                    KeyboardManager.IsActive--;
                }
            };

            item.ItemUri.Site.DataService.ExecuteAsync("SetSortOrder", completed, item.ItemUri.ItemId.ToString(), item.ItemUri.DatabaseName.Name, sortOrder);
        }

        protected abstract int GetSortOrder([NotNull] ItemTreeViewItem item);
    }
}
