// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Galleries
{
    public class FolderDescriptor
    {
        public FolderDescriptor([NotNull] string location)
        {
            Assert.ArgumentNotNull(location, nameof(location));

            Location = location;
        }

        [NotNull]
        public string Location { get; private set; }
    }
}
