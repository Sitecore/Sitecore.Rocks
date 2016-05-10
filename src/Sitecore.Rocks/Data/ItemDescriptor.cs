// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class ItemDescriptor : IItem
    {
        public ItemDescriptor([NotNull] ItemUri itemUri, [NotNull] string name)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(name, nameof(name));

            ItemUri = itemUri;
            Name = name;
            Icon = Icon.Empty;
        }

        public ItemDescriptor([NotNull] ItemUri itemUri, [NotNull] string name, [NotNull] Icon sectionIcon)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(sectionIcon, nameof(sectionIcon));

            ItemUri = itemUri;
            Name = name;
            Icon = sectionIcon;
        }

        public Icon Icon { get; }

        public ItemUri ItemUri { get; }

        public string Name { get; }
    }
}
