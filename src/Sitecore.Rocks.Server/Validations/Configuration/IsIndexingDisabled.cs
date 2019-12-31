// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Enable indexing", "Configuration")]
    public class IsIndexingDisabled : Validation
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

            if (validationHelper.UpdateIntervalSettingValue != TimeSpan.Zero)
            {
                return;
            }

            output.Write(SeverityLevel.Warning, "Enable indexing", "Indexing should be enabled to increase performance.", "To enable indexing, set the setting \"Indexing.UpdateInterval\" to a value other than \"00:00:00\" in the web.config.");
        }
    }
}
