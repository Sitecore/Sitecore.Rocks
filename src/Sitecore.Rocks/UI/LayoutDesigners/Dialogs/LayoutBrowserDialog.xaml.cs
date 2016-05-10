// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs
{
    public partial class LayoutBrowserDialog
    {
        public LayoutBrowserDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return LayoutSelector.DatabaseUri; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                LayoutSelector.DatabaseUri = value;
            }
        }

        [CanBeNull]
        public object SelectedItem => LayoutSelector.SelectedItem;

        [CanBeNull]
        public LayoutHeader SelectedLayout
        {
            get
            {
                var selectedItem = LayoutSelector.SelectedItem as ListBoxItem;
                if (selectedItem == null)
                {
                    return null;
                }

                return selectedItem.Tag as LayoutHeader;
            }
        }

        [NotNull]
        public string SelectedPath
        {
            get
            {
                var selectedItem = LayoutSelector.SelectedItem as ListBoxItem;
                if (selectedItem == null)
                {
                    return string.Empty;
                }

                var layoutHeader = selectedItem.Tag as LayoutHeader;
                if (layoutHeader == null)
                {
                    return string.Empty;
                }

                return layoutHeader.Path;
            }
        }

        public void Initialize([NotNull] string title, [NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            Title = title;
            DatabaseUri = databaseUri;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            EnableButtons();
        }

        private void EnableButtons()
        {
            OK.IsEnabled = LayoutSelector.SelectedItem != null;
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            OkClick(sender, e);
        }

        private void LayoutSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedLayout = SelectedLayout;
            if (selectedLayout != null)
            {
                LayoutSelector.AddToRecent(selectedLayout);
            }

            this.Close(true);
        }
    }
}
