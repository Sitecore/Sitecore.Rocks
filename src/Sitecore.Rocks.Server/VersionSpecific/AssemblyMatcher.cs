using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sitecore.Rocks.Server.VersionSpecific
{
    public class AssemblyMatcher
    {
        private static Regex VersionAssemblyRegex = new Regex("Sitecore.Rocks.Server.v([0-9]+\\.[0-9]+).dll");

        public AssemblyResource MatchResource(string[] resources, string currentVersionString)
        {
            var assemblies = FindAssemblyResources(resources);
            var currentVersion = new Version(currentVersionString);

            // Look for exact match, ignoring build and revision
            var assembly = assemblies.FirstOrDefault(x => x.Version.Major == currentVersion.Major &&
                x.Version.Minor == currentVersion.Minor);
            if (assembly != null)
            {
                return assembly;
            }

            assemblies = assemblies.OrderBy(x => x.Version);
            // Lower version
            if (currentVersion < assemblies.First().Version)
            {
                return assemblies.First();
            }
            // Higher version
            if (currentVersion > assemblies.Last().Version)
            {
                return assemblies.Last();
            }

            return null;
        }

        protected IEnumerable<AssemblyResource> FindAssemblyResources(string[] resources)
        {
            return resources
                .Select(name => new
                {
                    Name = name,
                    Match = VersionAssemblyRegex.Match(name)
                })
                .Where(x => x.Match.Success)
                .Select(x => new AssemblyResource
                {
                    ResourceName = x.Name,
                    Version = new Version(x.Match.Groups[1].Value)
                });
        }
    }
}