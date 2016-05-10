// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PackageManagers.Sources
{
    public static class PackageSourceManager
    {
        private static readonly List<PackageSourceDescriptor> sources = new List<PackageSourceDescriptor>();

        [NotNull]
        public static IEnumerable<PackageSourceDescriptor> Sources
        {
            get { return sources; }
        }

        public static void LoadType([NotNull] Type type, [NotNull] PackageSourceAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var source = Activator.CreateInstance(type) as IPackageSource;
            if (source == null)
            {
                return;
            }

            var descriptor = new PackageSourceDescriptor(source, attribute);

            sources.Add(descriptor);
        }

        public class PackageSourceDescriptor
        {
            public PackageSourceDescriptor([NotNull] IPackageSource source, [NotNull] PackageSourceAttribute attribute)
            {
                Assert.ArgumentNotNull(source, nameof(source));
                Assert.ArgumentNotNull(attribute, nameof(attribute));

                Source = source;
                Attribute = attribute;
            }

            [NotNull]
            public PackageSourceAttribute Attribute { get; private set; }

            [NotNull]
            public IPackageSource Source { get; private set; }
        }
    }
}
