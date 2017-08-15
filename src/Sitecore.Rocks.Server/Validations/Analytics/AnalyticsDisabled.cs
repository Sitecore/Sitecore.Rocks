// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel.License;

namespace Sitecore.Rocks.Server.Validations.Analytics
{
    [Validation("Disable Analytics on back-end sites", "Configuration")]
    public class AnalyticsDisabled : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            if (!Settings.GetBoolSetting("Analytics.Enabled", false) || !License.HasModule("Sitecore.OMS"))
            {
                return;
            }

            CheckSite(output, "shell");
            CheckSite(output, "login");
            CheckSite(output, "admin");
            CheckSite(output, "modules_shell");
            CheckSite(output, "scheduler");
            CheckSite(output, "system");
            CheckSite(output, "publisher");
        }

        public void CheckSite([NotNull] ValidationWriter output, [NotNull] string siteName)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(siteName, nameof(siteName));

            var site = Factory.GetSite(siteName);
            if (site == null)
            {
                output.Write(SeverityLevel.Hint, string.Format("Site \"{0}\" is missing", siteName), string.Format("The site \"{0}\" is a back-end site and is expected to be defined in the web.config.", siteName), "Either define the site or hide this message.");
            }
        }
    }
}
