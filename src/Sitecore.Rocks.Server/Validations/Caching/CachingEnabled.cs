// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Caching
{
    [Validation("Enable caching", "Performance")]
    public class CachingEnabled : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            if (Settings.Caching.Enabled)
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Enable caching", "Caching is disabled.", "Set the setting \"Caching.Enabled\" to true in the web.config.");
        }
    }
}
