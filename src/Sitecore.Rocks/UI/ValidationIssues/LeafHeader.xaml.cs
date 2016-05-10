// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.ValidationIssues
{
    public partial class LeafHeader
    {
        private Icon icon;

        public LeafHeader()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public Icon Icon
        {
            get { return icon; }

            set
            {
                icon = value;

                if (value != null)
                {
                    IconImage.Source = value.GetSource();
                }
            }
        }

        [NotNull]
        public string Text
        {
            get { return TextBlock.Text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                TextBlock.Text = value;
            }
        }

        public event MouseButtonEventHandler DoubleClick;

        private void HandleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.ClickCount < 2)
            {
                return;
            }

            var click = DoubleClick;
            if (click != null)
            {
                click(this, e);
            }
        }
    }
}
