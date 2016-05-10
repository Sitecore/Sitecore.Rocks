// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public partial class TabItemHeader
    {
        private string _header;

        public TabItemHeader()
        {
            InitializeComponent();
        }

        [NotNull]
        public string Header
        {
            get { return _header; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _header = value;

                TabHeader.Text = value;
            }
        }

        public event EventHandler Click;

        private void HandleClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var click = Click;
            if (click != null)
            {
                click(this, EventArgs.Empty);
            }
        }
    }
}
