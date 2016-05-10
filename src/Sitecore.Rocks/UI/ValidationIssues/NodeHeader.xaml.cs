// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.ValidationIssues
{
    public partial class NodeHeader
    {
        private Icon icon;

        private int leafCount;

        public NodeHeader()
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

        public int LeafCount
        {
            get { return leafCount; }

            set
            {
                leafCount = value;

                if (value == 0)
                {
                    Leaves.Text = string.Empty;
                    return;
                }

                if (value == 1)
                {
                    Leaves.Text = Rocks.Resources.TreeViewItemHeader_LeafCount__1_issue_;
                    return;
                }

                Leaves.Text = string.Format(Rocks.Resources.TreeViewItemHeader_LeafCount___0__issues_, value);
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
    }
}
