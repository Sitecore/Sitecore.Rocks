// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Extensions.AssemblyNameExtensions
{
    [UsedImplicitly]
    public static class AssemblyExtensions
    {
        [NotNull, UsedImplicitly]
        public static Version GetFileVersion([NotNull] this Assembly assembly)
        {
            Assert.ArgumentNotNull(assembly, nameof(assembly));

            if (!File.Exists(assembly.Location))
            {
                return new Version(0, 0);
            }

            var info = FileVersionInfo.GetVersionInfo(assembly.Location);

            return new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart);
        }

        [NotNull]
        public static IEnumerable<object> GetResourcePaths([NotNull] this Assembly assembly)
        {
            Assert.ArgumentNotNull(assembly, nameof(assembly));

            var culture = Thread.CurrentThread.CurrentCulture;
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, assembly);

            try
            {
                var resourceSet = resourceManager.GetResourceSet(culture, true, true);

                foreach (DictionaryEntry resource in resourceSet)
                {
                    yield return resource.Key;
                }
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
        }

        public static bool ResourceExists([NotNull] this Assembly assembly, [NotNull] string resourcePath)
        {
            Assert.ArgumentNotNull(assembly, nameof(assembly));
            Assert.ArgumentNotNull(resourcePath, nameof(resourcePath));

            if (resourcePath.StartsWith("/"))
            {
                resourcePath = resourcePath.Mid(1);
            }

            return assembly.GetResourcePaths().Contains(resourcePath.ToLowerInvariant());
        }
    }
}
