using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Server.VersionSpecific;
using Xunit;

namespace Sitecore.Rocks.Server.Tests.VersionSpecific
{
    public class AssemblyMatcherTests
    {
        private const string AssemblyPrefix = "Sitecore.Rocks.Server.";

        [Fact]
        public void FiltersOutNonAssemblies()
        {
            var testable = new Testable();
            var resources = new[]
            {
                $"Sitecore.Rocks.Blah.{AssemblyPrefix}v1.1.dll",
                "Sitecore.Rocks.Blah.somethingelse.txt",
                "Sitecore.Rocks.Blah.prettyicon.jpg",
            };

            var matches = testable.TestFindAssemblyResources(resources);

            Assert.Single(matches);
            Assert.Single(matches, x => x.ResourceName == resources[0]);
        }

        [Theory]
        [InlineData("7.5", 7, 5)]
        [InlineData("9.10", 9, 10)]
        [InlineData("10.1", 10, 1)]
        public void ParsesVersionNumbers(string assemblyVersion, int expectedMajor, int expectedMinor)
        {
            var expected = new Version(expectedMajor, expectedMinor);
            var testable = new Testable();
            var resources = new[]
            {
                $"Blah.{AssemblyPrefix}v{assemblyVersion}.dll"
            };

            var matches = testable.TestFindAssemblyResources(resources);

            Assert.Single(matches, x => x.Version == expected);
        }

        [Fact]
        public void FindsExactMatch()
        {
            var matcher = new AssemblyMatcher();
            var resources = new[]
            {
                $"Blah.{AssemblyPrefix}v9.1.dll",
                $"Blah.{AssemblyPrefix}v9.2.dll"
            };

            var match = matcher.MatchResource(resources, "9.2.1");

            Assert.Equal(resources[1], match.ResourceName);
        }

        [Fact]
        public void MatchesLowerVersionToLowestAssembly()
        {
            var matcher = new AssemblyMatcher();
            var resources = new[]
            {
                $"Blah.{AssemblyPrefix}v9.2.dll",
                $"Blah.{AssemblyPrefix}v9.1.dll",
            };

            var match = matcher.MatchResource(resources, "7.5.5");

            Assert.Equal(resources[1], match.ResourceName);
        }

        [Fact]
        public void MatchesHigherVersionToHighestAssembly()
        {
            var matcher = new AssemblyMatcher();
            var resources = new[]
            {
                $"Blah.{AssemblyPrefix}v9.2.dll",
                $"Blah.{AssemblyPrefix}v9.1.dll",
            };

            var match = matcher.MatchResource(resources, "10.2.1");

            Assert.Equal(resources[0], match.ResourceName);
        }

        protected class Testable : AssemblyMatcher
        {
            public IEnumerable<AssemblyResource> TestFindAssemblyResources(string[] resources)
            {
                return base.FindAssemblyResources(resources);
            }
        }
    }
}
