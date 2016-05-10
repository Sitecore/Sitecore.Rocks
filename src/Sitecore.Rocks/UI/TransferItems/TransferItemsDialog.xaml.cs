// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls.ItemDependencyListViews;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.TransferItems
{
    public partial class TransferItemsDialog
    {
        public TransferItemsDialog([NotNull] ItemTreeViewItem target, [NotNull] IEnumerable<IItem> items)
        {
            Assert.ArgumentNotNull(target, nameof(target));
            Assert.ArgumentNotNull(items, nameof(items));

            InitializeComponent();
            this.InitializeDialog();

            Items = items;
            ItemDependencies.ItemsSource = items.Select(i => i.ItemUri);
        }

        [NotNull]
        public IEnumerable<ItemDependencyListView.ItemDescriptor> SelectedItemDescriptors
        {
            get { return ItemDependencies.SelectedItemDescriptors; }
        }

        [NotNull]
        public IEnumerable<ItemUri> SelectedItems
        {
            get { return ItemDependencies.SelectedItems; }
        }

        [NotNull]
        protected IEnumerable<IItem> Items { get; set; }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
