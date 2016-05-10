// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Packages
{
    public class SitecoreNuGetPackageBuilder : NuGetPackageBuilderBase
    {
        public SitecoreNuGetPackageBuilder([NotNull] string fileName, [NotNull] string nugetPath) : base(fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(nugetPath, nameof(nugetPath));

            NuGetPath = nugetPath;
        }

        [NotNull]
        public string NuGetPath { get; }

        protected override void WriteFiles(XmlTextWriter output)
        {
            Debug.ArgumentNotNull(output, nameof(output));

            WriteFile(output, Path.Combine(NuGetPath, "init.ps1"), "tools\\init.ps1");
            WriteFile(output, Path.Combine(NuGetPath, "install.ps1"), "tools\\install.ps1");
            WriteFile(output, Path.Combine(NuGetPath, "Sitecore.NuGet.1.0.dll"), "tools\\Sitecore.NuGet.1.0.dll");
            WriteFile(output, Path.Combine(NuGetPath, "uninstall.ps1"), "tools\\uninstall.ps1");
        }

        private void WriteFile([NotNull] XmlTextWriter output, [NotNull] string src, [NotNull] string target)
        {
            Debug.ArgumentNotNull(target, nameof(target));
            Debug.ArgumentNotNull(src, nameof(src));
            Debug.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("file");
            output.WriteAttributeString("src", src);
            output.WriteAttributeString("target", target);
            output.WriteEndElement();
        }
    }
}
