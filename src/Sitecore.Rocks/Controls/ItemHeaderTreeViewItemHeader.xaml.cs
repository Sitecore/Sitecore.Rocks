// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public partial class ItemHeaderTreeViewItemHeader
    {
        public ItemHeaderTreeViewItemHeader()
        {
            InitializeComponent();
        }

        public void Initialize([NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            ItemName.Text = itemHeader.Name;
            ItemPath.Text = itemHeader.Path;
            TemplateName.Text = @"(" + itemHeader.TemplateName + @")";
        }
    }
}
