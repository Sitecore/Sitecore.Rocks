// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries
{
    public class GalleryDescriptor
    {
        public GalleryDescriptor([NotNull] string name, [NotNull] string location)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(location, nameof(location));

            Name = name;
            Location = location;
        }

        [NotNull]
        public string Location { get; private set; }

        [NotNull]
        public string Name { get; private set; }
    }
}
