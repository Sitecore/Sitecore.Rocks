// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Disable debugging in web.config", "Performance")]
    public class WebConfigDebug : Validation
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

            var node = doc.SelectSingleNode("/configuration/system.web/compilation");
            if (node == null)
            {
                severity = SeverityLevel.Hint;
                problem = "The configuration node \"/configuration/system.web/compilation\" was not found in the web.config file.";
            }
            else
            {
                var value = XmlUtil.GetAttribute("debug", node);

                if (value == "true")
                {
                    severity = SeverityLevel.Suggestion;
                    problem = "The \"debug\" attribute of the compilation element is set to \"true\". This setting may decrease performance.";
                }
            }

            if (severity == SeverityLevel.None)
            {
                return;
            }

            output.Write(severity, "Disable debugging in web.config", problem, "It is recommended to set the \"debug\" attribute to \"false\".");
        }
    }
}
