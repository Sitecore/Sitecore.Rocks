// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class LayoutHeader
    {
        public LayoutHeader([NotNull] ItemUri layoutUri, [NotNull] string name, [NotNull] string icon, [NotNull] string path, [NotNull] string section)
        {
            Assert.ArgumentNotNull(layoutUri, nameof(layoutUri));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(icon, nameof(icon));
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(section, nameof(section));

            Icon = icon;
            LayoutUri = layoutUri;
            Name = name;
            Path = path;
            Section = section;

            if (!string.IsNullOrEmpty(Path))
            {
                ParentPath = (System.IO.Path.GetDirectoryName(Path) ?? string.Empty).Replace("\\", "/");
            }
            else
            {
                ParentPath = string.Empty;
            }
        }

        [NotNull]
        public string Icon { get; private set; }

        [NotNull]
        public ItemId LayoutId
        {
            get { return LayoutUri.ItemId; }
        }

        [NotNull]
        public ItemUri LayoutUri { get; }

        [NotNull]
        public string Name { get; private set; }

        [NotNull]
        public string ParentPath { get; private set; }

        [NotNull]
        public string Path { get; }

        [NotNull]
        public string Section { get; private set; }
    }
}
