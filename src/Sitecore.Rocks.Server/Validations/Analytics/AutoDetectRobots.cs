// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel.License;

namespace Sitecore.Rocks.Server.Validations.Analytics
{
    [Validation("Enable robot autodetection", "Performance")]
    public class AutoDetectRobots : Validation
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

            if (!Settings.GetBoolSetting("Analytics.AutoDetectBots", true))
            {
                output.Write(SeverityLevel.Suggestion, "Enable robot autodetection", "Autodetection of robots is disabled. Robots may take up as much as 90% of all traffic on a web site causing sever load on the web server and database server.", "Set the setting \"Analytics.AutoDetectBots\" to true in the web.config.");
            }
        }
    }
}
