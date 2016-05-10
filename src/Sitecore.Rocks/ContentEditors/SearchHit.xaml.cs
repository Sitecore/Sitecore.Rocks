// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public partial class SearchHit
    {
        private ItemHeader itemHeader;

        public SearchHit()
        {
            InitializeComponent();
        }

        [NotNull]
        public ItemHeader ItemHeader
        {
            get { return itemHeader; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                itemHeader = value;
                Update();
            }
        }

        public void Load([NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            ItemHeader = itemHeader;
        }

        private void Update()
        {
            ItemName.Text = ItemHeader.Name;

            var templateName = ItemHeader.TemplateName;
            if (!string.IsNullOrEmpty(templateName))
            {
                TemplateName.Text = string.Format(@"[{0}]", templateName);
            }

            var path = ItemHeader.Path;

            var n = path.LastIndexOf(@"/", StringComparison.Ordinal);
            if (n >= 0)
            {
                path = path.Substring(0, n);
            }

            if (string.IsNullOrEmpty(path))
            {
                path = @"/";
            }

            Path.Text = string.Format(Rocks.Resources.SearchHit_Update__in__0__, path);
            Icon.Source = ItemHeader.Icon.GetSource();
        }
    }
}
