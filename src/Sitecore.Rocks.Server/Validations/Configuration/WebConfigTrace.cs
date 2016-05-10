// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Disable tracing web.config", "Configuration")]
    public class WebConfigTrace : Validation
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

            var node = doc.SelectSingleNode("/configuration/system.web/trace");
            if (node == null)
            {
                severity = SeverityLevel.Hint;
                problem = "The configuration node \"/configuration/system.web/trace\" was not found in the web.config file.";
            }
            else
            {
                var value = XmlUtil.GetAttribute("enabled", node);

                if (value == "true")
                {
                    severity = SeverityLevel.Suggestion;
                    problem = "The \"enabled\" attribute of \"/configuration/system.web/trace\" is set to \"true\". Tracing should only be enabled when debugging.";
                }
            }

            if (severity == SeverityLevel.None)
            {
                return;
            }

            output.Write(severity, "Disable tracing web.config", problem, "Set the \"enabled\" attribute of \"/configuration/system.web/trace\" to \"false\" in the web.config.");
        }
    }
}
