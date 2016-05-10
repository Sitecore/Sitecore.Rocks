// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.ContentTrees.Gutters;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems
{
    public class LibraryItemTreeViewItem : BaseTreeViewItem, IItem
    {
        public LibraryItemTreeViewItem(IItem item)
        {
            Item = item;

            Text = Item.Name;
            Margin = new Thickness(0, 2, 0, 0);
            Icon = item.Icon;
            ToolTip = Item.Name;

            MouseDown += Locate;
            MouseDoubleClick += Open;
            ToolTipOpening += OpenToolTip;

            Notifications.RegisterItemEvents(this, deleted: ItemDeleted, renamed: ItemRenamed);
        }

        [NotNull]
        public IItem Item { get; }

        public ItemUri ItemUri
        {
            get { return Item.ItemUri; }
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri deletedItemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(deletedItemUri, nameof(deletedItemUri));

            if (Item.ItemUri == deletedItemUri)
            {
                Remove();
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri renamedItemUri, [NotNull] string newName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(renamedItemUri, nameof(renamedItemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            if (Item.ItemUri == renamedItemUri)
            {
                ItemHeader.Text = newName;
                GutterManager.UpdateGutter(renamedItemUri);
            }
        }

        private void Locate([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                return;
            }

            var treeView = this.GetAncestorOrSelf<ItemTreeView>();
            if (treeView != null)
            {
                treeView.ExpandTo(Item.ItemUri);
            }
        }

        private void Open([NotNull] object sender, [NotNull] MouseButtonEventArgs mouseButtonEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(mouseButtonEventArgs, nameof(mouseButtonEventArgs));

            AppHost.OpenContentEditor(Item.ItemUri);
        }

        private void OpenToolTip([NotNull] object sender, [NotNull] ToolTipEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ToolTip = ToolTipBuilder.BuildToolTip(Item);
        }
    }
}
