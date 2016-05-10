// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.IO;

namespace Sitecore.Rocks.Projects.ProjectItems
{
    public class ProjectSiteItem : ProjectItemBase
    {
        public ProjectSiteItem([NotNull] ProjectBase project) : base(project)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            HostName = string.Empty;
            UserName = string.Empty;
        }

        [NotNull]
        public string HostName { get; set; }

        public bool IsRemoteSitecore { get; set; }

        [NotNull]
        public string UserName { get; set; }

        public override void Load(XElement projectElement)
        {
            Assert.ArgumentNotNull(projectElement, nameof(projectElement));

            HostName = projectElement.GetAttributeValue("Server");
            UserName = projectElement.GetAttributeValue("UserName");
            IsRemoteSitecore = string.Compare(projectElement.GetAttributeValue("IsRemoteSitecore"), @"True", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override void Save(OutputWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("PropertyGroup");
            output.WriteStartElement("Site");

            output.WriteAttributeString(@"Server", HostName);
            output.WriteAttributeString(@"UserName", UserName);
            output.WriteAttributeString(@"IsRemoteSitecore", IsRemoteSitecore ? @"True" : @"False");

            output.WriteEndElement();
            output.WriteEndElement();
        }
    }
}
