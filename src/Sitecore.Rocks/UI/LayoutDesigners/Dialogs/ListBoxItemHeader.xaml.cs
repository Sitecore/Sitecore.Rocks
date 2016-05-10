// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs
{
    public partial class ListBoxItemHeader
    {
        public ListBoxItemHeader([NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            InitializeComponent();

            var path = itemHeader.Path;
            var n = path.LastIndexOf('/');
            if (n >= 0)
            {
                path = path.Left(n);
            }

            HeaderTextBox.Text = itemHeader.Name;
            PathTextBox.Text = string.Format("(in {0})", path);
        }
    }
}
