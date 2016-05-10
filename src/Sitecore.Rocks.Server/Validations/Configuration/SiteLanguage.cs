// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Site language", "Configuration")]
    public class SiteLanguage : Validation
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
                var site = Factory.GetSite(siteName);

                if (site.Database == null || string.IsNullOrEmpty(site.Language))
                {
                    continue;
                }

                if (LanguageManager.IsLanguageNameDefined(site.Database, site.Language))
                {
                    continue;
                }

                output.Write(SeverityLevel.Error, "Site language", string.Format("The language \"{0}\" is specified on the site \"{1}\" but is not defined in the site database \"{2}\".", site.Language, site.Name, site.Database.Name), string.Format("Set the \"Language\" attribute on the site \"{0}\" to an existing language.", site.Name));
            }
        }
    }
}
