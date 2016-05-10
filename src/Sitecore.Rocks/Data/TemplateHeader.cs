// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class TemplateHeader
    {
        public TemplateHeader([NotNull] ItemUri templateUri, [NotNull] string name, [NotNull] string icon, [NotNull] string path, [NotNull] string section, bool isBranch)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(icon, nameof(icon));
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(section, nameof(section));

            Path = path;
            Icon = icon;
            IsBranch = isBranch;
            Name = name;
            Section = section;
            TemplateUri = templateUri;

            if (!string.IsNullOrEmpty(Path))
            {
                ParentPath = AppHost.Files.GetDirectoryName(Path).Replace("\\", "/");
            }
            else
            {
                ParentPath = string.Empty;
            }
        }

        [NotNull]
        public string Icon { get; private set; }

        public bool IsBranch { get; private set; }

        [NotNull]
        public string Name { get; private set; }

        [NotNull]
        public string ParentPath { get; private set; }

        [NotNull]
        public string Path { get; }

        [NotNull]
        public string Section { get; private set; }

        [NotNull]
        public ItemId TemplateId => TemplateUri.ItemId;

        [NotNull]
        public ItemUri TemplateUri { get; }
    }
}
