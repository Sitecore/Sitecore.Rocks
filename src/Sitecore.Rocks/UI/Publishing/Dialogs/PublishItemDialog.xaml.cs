// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Publishing.Dialogs
{
    public partial class PublishItemDialog
    {
        public PublishItemDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            JobViewer.IsChecked = AppHost.Settings.Options.ShowJobViewer;
        }

        [NotNull]
        public string Caption
        {
            get { return Title ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Title = value;
            }
        }

        [NotNull]
        public IEnumerable<ItemUri> SelectedItems
        {
            get { return ItemDependencies.SelectedItems; }
        }

        protected IEnumerable<IItem> Items { get; set; }

        public void Initialize([NotNull] IEnumerable<IItem> items, [NotNull] string caption, [NotNull] string publishingText)
        {
            Assert.ArgumentNotNull(items, nameof(items));
            Assert.ArgumentNotNull(caption, nameof(caption));
            Assert.ArgumentNotNull(publishingText, nameof(publishingText));

            Caption = caption;

            Items = items;

            ItemDependencies.ItemsSource = items.Select(i => i.ItemUri);
        }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Options.ShowJobViewer = JobViewer.IsChecked == true;
            AppHost.Settings.Options.Save();

            this.Close(true);
        }
    }
}
