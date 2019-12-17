// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Enable server specific properties for indexing", "Configuration")]
    public class IndexingServerSpecificProperties : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            var validationHelper = VersionSpecific.Services.LegacyIndexingValidator;

            if (!validationHelper.ShouldValidate)
            {
                return;
            }

            if (validationHelper.UpdateIntervalSettingValue == TimeSpan.Zero)
            {
                return;
            }

            if (validationHelper.ServerSpecificPropertiesSettingValue)
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Enable server specific properties for indexing", "When indexing is enabled, it is also recommended that server specific properties are enabled.", "To enable server specific properties, set the setting \"Indexing.ServerSpecificProperties\" to \"true\" in the web.config.");
        }
    }
}
