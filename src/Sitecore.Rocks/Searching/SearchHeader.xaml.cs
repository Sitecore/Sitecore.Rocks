// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Searching
{
    public partial class SearchHeader
    {
        private int itemCount;

        public SearchHeader()
        {
            InitializeComponent();
        }

        public int ItemCount
        {
            get { return itemCount; }

            set
            {
                itemCount = value;

                if (value == 1)
                {
                    Leaves.Text = Rocks.Resources.SearchHeader_ItemCount__1_item;
                }
                else
                {
                    Leaves.Text = string.Format(Rocks.Resources.SearchHeader_ItemCount__0__items, value);
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
    }
}
