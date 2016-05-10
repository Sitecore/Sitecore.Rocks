// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Configure mail server in web.config", "Configuration")]
    public class WebConfigMailServer : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            if (!string.IsNullOrEmpty(Settings.MailServer))
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Configure mail server in web.config", "The mail server settings in web.config are missing.", "It is recommended to set the \"MailServer\", \"MailServerPort\", \"MailServerUserName\" and \"MailServerPassword\" settings in web.config.");
        }
    }
}
