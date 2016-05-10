// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Reflection;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Validations.Configuration
{
    [Validation("Referenced types in web.config", "Configuration")]
    public class WebConfigTypes : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            var doc = Factory.GetConfiguration();

            var list = doc.SelectNodes("//*[@type]");
            foreach (XmlNode node in list)
            {
                var typeString = XmlUtil.GetAttribute("type", node);
                if (string.IsNullOrEmpty(typeString))
                {
                    continue;
                }

                if (typeString == "both")
                {
                    continue;
                }

                typeString = typeString.Replace(", mscorlib", string.Empty);
                typeString = typeString.Replace(",mscorlib", string.Empty);

                Type type;
                try
                {
                    type = ReflectionUtil.GetTypeInfo(typeString);
                }
                catch
                {
                    type = null;
                }

                if (type != null)
                {
                    continue;
                }

                var path = XmlUtil.GetPath(node);
                output.Write(SeverityLevel.Error, "Referenced types in web.config", string.Format("The referenced type \"{0}\" does not exist. It is referenced from {1}.", typeString, path), "Either correct the reference or remove it.");
            }
        }
    }
}
