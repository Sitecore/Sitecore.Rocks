// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Skins.DbBrowser
{
    public partial class Section
    {
        public Section()
        {
            InitializeComponent();
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
