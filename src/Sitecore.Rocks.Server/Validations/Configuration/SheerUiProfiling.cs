// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Is Sheer UI profiling enabled", "Configuration")]
    public class SheerUiProfiling : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            if (!Settings.Profiling.SheerUI)
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Sheer UI profiling", "Sheer UI profiling is enabled.", "It is recommended that Sheer UI Profiling is disabled. To disable Sheer UI Profiling, open the web.config and set the setting \"Profiling.SheerUI\" to \"false\".");
        }
    }
}
