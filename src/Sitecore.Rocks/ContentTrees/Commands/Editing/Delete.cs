// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Dialogs.DeleteItemsDialogs;
using Sitecore.Rocks.ContentTrees.VirtualItems.Favorites;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command, CommandId(CommandIds.SitecoreExplorer.Delete, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Editing, Icon = "Resources/16x16/delete.png", Priority = 0x0120)]
    public class Delete : CommandBase
    {
        public Delete()
        {
            Text = Resources.Delete_Delete_Delete;
            Group = "Edit";
            SortingValue = 3000;
            Icon = new Icon("Resources/16x16/delete.png");
        }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is IDeleteItemContext))
            {
                return false;
            }

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            if (!context.Items.All(item => item is ICanDelete))
            {
                return false;
            }

            var texts = context.Items.OfType<ICanDeleteWithText>().FirstOrDefault();
            if (texts != null)
            {
                Text = texts.CommandText;
            }
            else
            {
                Text = Resources.Delete_Delete_Delete;
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

            var selectedItems = context.Items.OfType<ICanDelete>().ToList();
            if (selectedItems.Count != context.Items.Count())
            {
                return;
            }

            if (selectedItems.All(i => i is IItem && !(i is FavoriteTreeViewItem)))
            {
                var items = selectedItems.OfType<IItem>().ToList();

                var item = items.First();
                if (item.ItemUri.Site.CanExecute)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        DeleteItemsQuickly(items);
                    }
                    else
                    {
                        DeleteItems(items);
                    }

                    return;
                }
            }

            DeleteOneByOne(selectedItems);
        }

        private void DeleteItems([NotNull] IEnumerable<IItem> selectedItems)
        {
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            var d = new DeleteItemsDialog(selectedItems);
            AppHost.Shell.ShowDialog(d);
        }

        private void DeleteItemsQuickly([NotNull] List<IItem> selectedItems)
        {
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            var singleText = Resources.Delete_Execute_Are_you_sure_you_want_to_delete___0___;
            var multipleText = Resources.Delete_Execute_Are_you_sure_you_want_to_delete_these__0__items_;

            var texts = selectedItems.OfType<ICanDeleteWithText>().FirstOrDefault();
            if (texts != null)
            {
                singleText = texts.SingleText;
                multipleText = texts.SingleText;
            }

            if (selectedItems.Count() == 1)
            {
                var item = selectedItems.First() as ICanDelete;
                if (item == null)
                {
                    return;
                }

                if (AppHost.MessageBox(string.Format(singleText, item.Text), Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
                {
                    return;
                }
            }
            else
            {
                if (AppHost.MessageBox(string.Format(multipleText, selectedItems.Count), Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            Site.RequestCompleted completed = delegate
            {
                foreach (var item in selectedItems)
                {
                    Notifications.RaiseItemDeleted(this, item.ItemUri);
                }
            };

            var items = string.Join("|", selectedItems.Select(i => i.ItemUri.ItemId.ToString()));
            var selectedItem = selectedItems.First();

            selectedItem.ItemUri.Site.Execute("Items.DeleteItems", completed, selectedItem.ItemUri.DatabaseName.ToString(), items, false, true);
        }

        private void DeleteOneByOne([NotNull] List<ICanDelete> selectedItems)
        {
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            var singleText = Resources.Delete_Execute_Are_you_sure_you_want_to_delete___0___;
            var multipleText = Resources.Delete_Execute_Are_you_sure_you_want_to_delete_these__0__items_;

            var texts = selectedItems.OfType<ICanDeleteWithText>().FirstOrDefault();
            if (texts != null)
            {
                singleText = texts.SingleText;
                multipleText = texts.SingleText;
            }

            if (selectedItems.Count == 1)
            {
                var item = selectedItems[0];

                if (AppHost.MessageBox(string.Format(singleText, item.Text), Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
                {
                    return;
                }
            }
            else
            {
                if (AppHost.MessageBox(string.Format(multipleText, selectedItems.Count), Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            foreach (var item in selectedItems)
            {
                item.Delete(false);
                RemoveTreeViewItem(item);
            }
        }

        private void RemoveTreeViewItem([NotNull] ICanDelete item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var treeViewItem = item as TreeViewItem;
            if (treeViewItem == null)
            {
                return;
            }

            var parent = treeViewItem.Parent as TreeViewItem;
            if (parent == null)
            {
                return;
            }

            parent.Items.Remove(treeViewItem);
        }
    }
}
