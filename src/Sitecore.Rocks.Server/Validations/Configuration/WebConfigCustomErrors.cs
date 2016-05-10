// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Enable Custom Errors in web.config", "Security")]
    public class WebConfigCustomErrors : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            var severity = SeverityLevel.None;
            var problem = string.Empty;

            var doc = new XmlDocument();
            doc.Load(FileUtil.MapPath("/web.config"));

            var node = doc.SelectSingleNode("/configuration/system.web/customErrors");
            if (node == null)
            {
                severity = SeverityLevel.Hint;
                problem = "The configuration node \"/configuration/system.web/customErrors\" was not found in the web.config file.";
            }
            else
            {
                var value = XmlUtil.GetAttribute("enabled", node);

                if (value == "Off")
                {
                    severity = SeverityLevel.Error;
                    problem = "The \"mode\" attribute of the \"/configuration/system.web/customErrors\" element is set to \"Off\". This will show exceptions to end-users and is a potential security risk since a stack-trace is shown.";
                }
            }

            if (severity == SeverityLevel.None)
            {
                return;
            }

            output.Write(severity, "Enable Custom Errors in web.config", problem, "Set \"mode\" attribute of the \"/configuration/system.web/customErrors\" element to \"RemoteOnly\" or \"On\".");
        }
    }
}
