// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Valid web stylesheet file", "Configuration")]
    public class WebConfigWebStylesheet : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            if (string.IsNullOrEmpty(Settings.WebStylesheet))
            {
                return;
            }

            if (FileUtil.Exists(Settings.WebStylesheet))
            {
                return;
            }

            output.Write(SeverityLevel.Warning, "Valid web stylesheet file", string.Format("The \"WebStylesheet\" setting in the web.config points to the non-existing file: {0}", Settings.WebStylesheet), "Either create the file or set the setting \"WebStylesheet\" to blank.");
        }
    }
}
