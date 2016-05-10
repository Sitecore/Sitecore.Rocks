// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Toolbars
{
    public partial class SkinButton
    {
        private string label;

        public SkinButton()
        {
            InitializeComponent();
        }

        [NotNull]
        public string Label
        {
            get { return label; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                label = value;
                LabelTextBlock.Text = value;
            }
        }
    }
}
