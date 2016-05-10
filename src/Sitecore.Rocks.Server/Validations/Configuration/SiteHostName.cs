// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Sites;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Duplicate site host names", "Configuration")]
    public class SiteHostName : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            var hostNames = new Dictionary<string, SiteContext>();

            foreach (var siteName in Factory.GetSiteNames())
            {
                var site = Factory.GetSite(siteName);

                SiteContext s;

                if (!hostNames.TryGetValue(site.Name, out s))
                {
                    hostNames[site.Name] = site;
                    continue;
                }

                output.Write(SeverityLevel.Error, "Duplicate site host names", string.Format("The host name \"{0}\" is used by both the \"{1}\" and the \"{2}\" site.", site.HostName, site.Name, s.Name), string.Format("Set the \"hostName\" attribute in the web.config on the \"{0}\" or the \"{1}\" site to a different host name.", site.Name, s.Name));
            }
        }
    }
}
