// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Caching
{
    [Validation("Enable Html caching", "Performance")]
    public class HtmlCache : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            foreach (var siteName in Factory.GetSiteNames())
            {
                switch (siteName.ToLowerInvariant())
                {
                    case "shell":
                    case "login":
                    case "admin":
                    case "service":
                    case "modules_shell":
                    case "modules_website":
                    case "scheduler":
                    case "system":
                    case "testing":
                    case "publisher":
                        continue;
                }

                var site = Factory.GetSite(siteName);
                if (site == null)
                {
                    continue;
                }

                if (!site.CacheHtml)
                {
                    output.Write(SeverityLevel.Suggestion, "Enable Html caching", string.Format("Html caching is disabled for the site \"{0}\". Performance may suffer.", siteName), string.Format("Set the \"cacheHtml\" setting to \"true\" for the site \"{0}\" in the web.config.", siteName));
                }
            }
        }
    }
}
