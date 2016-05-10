// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Dialogs
{
    public partial class AddDependenciesDialog
    {
        public AddDependenciesDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        [NotNull]
        public IEnumerable<ItemUri> SelectedItems
        {
            get { return ItemDependencies.SelectedItems; }
        }

        [CanBeNull]
        protected IEnumerable<IItem> Items { get; set; }

        public void Initialize([NotNull] IEnumerable<IItem> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            Items = items;

            ItemDependencies.ItemsSource = items.Select(i => i.ItemUri);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
