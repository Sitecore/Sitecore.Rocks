// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.InfoPanes
{
    public partial class InfoPaneTextBlock
    {
        public InfoPaneTextBlock()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string Header
        {
            get { return Label.Text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                Label.Text = value;
            }
        }

        [NotNull]
        public string Value
        {
            get { return ValueTextBox.Text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                ValueTextBox.Text = value;
                ValueTextBox.ToolTip = value;
            }
        }

        public event EventHandler Click;

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            if (Click != null)
            {
                return;
            }

            Link.Cursor = Cursors.Arrow;
            LinkIcon.Visibility = Visibility.Collapsed;
        }

        private void CopyToClipboard([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Clipboard.SetText(Value);
        }

        private void RaiseClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var click = Click;
            if (click != null)
            {
                click(this, e);
            }
        }
    }
}
