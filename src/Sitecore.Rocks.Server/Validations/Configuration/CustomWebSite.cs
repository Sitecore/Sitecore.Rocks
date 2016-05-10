// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Custom web site defined after \"website\" site", "Configuration")]
    public class CustomWebSite : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            var xmlDocument = Factory.GetConfiguration();

            var node = xmlDocument.SelectSingleNode("/sitecore/sites");
            if (node == null)
            {
                return;
            }

            var foundWebSite = false;
            foreach (XmlNode childNode in node.ChildNodes)
            {
                var siteName = XmlUtil.GetAttribute("name", childNode);
                if (siteName == "website")
                {
                    foundWebSite = true;
                    continue;
                }

                if (!foundWebSite)
                {
                    continue;
                }

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

                output.Write(SeverityLevel.Error, "Custom web site defíned after \"website\" site", string.Format("The web site \"{0}\" is defined after the \"website\" site. This breaks the Site Resolver.", siteName), string.Format("Move the \"{0}\" web site before the \"website\" site in the web.config.", siteName));
            }
        }
    }
}
