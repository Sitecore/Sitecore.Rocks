// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Diagnostics;
using System.Reflection;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Extensions.AssemblyExtensions
{
    public static class AssemblyExtensions
    {
        [NotNull]
        public static Version GetFileVersion([NotNull] this Assembly assembly)
        {
            Assert.ArgumentNotNull(assembly, nameof(assembly));

            var info = FileVersionInfo.GetVersionInfo(assembly.Location);

            return new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart);
        }
    }
}
