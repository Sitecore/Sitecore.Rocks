// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class FieldTypeHeader
    {
        public FieldTypeHeader([NotNull] ItemUri itemUri, [NotNull] string name, [NotNull] string icon, [NotNull] string path, [NotNull] string section)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(icon, nameof(icon));
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(section, nameof(section));

            Icon = icon;
            Name = name;
            Path = path;
            Section = section;
            ItemUri = itemUri;
        }

        [NotNull]
        public string Icon { get; private set; }

        [NotNull]
        public ItemUri ItemUri { get; private set; }

        [NotNull]
        public string Name { get; private set; }

        [NotNull]
        public string Path { get; private set; }

        [NotNull]
        public string Section { get; private set; }
    }
}
