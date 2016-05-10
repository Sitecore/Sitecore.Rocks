// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.Controls
{
    public partial class RenderingButton
    {
        private Icon _icon;

        private string _label;

        public RenderingButton()
        {
            InitializeComponent();

            Icon = Icon.Empty;
        }

        [NotNull]
        public Icon Icon
        {
            get { return _icon; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _icon = value;
                IconImage.Source = value.GetSource();
            }
        }

        [NotNull]
        public string Label
        {
            get { return _label; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _label = value;
                LabelTextBlock.Text = value;
            }
        }
    }
}
