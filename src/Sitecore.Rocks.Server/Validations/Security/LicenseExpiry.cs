// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel.License;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Validations.Security
{
    [Validation("License about to expire", "Licensing")]
    public class LicenseExpiry : Validation
    {
        public override bool CanCheck(string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            var license = License.VerifiedLicense();
            if (license == null)
            {
                return;
            }

            foreach (var module in GetModules(license))
            {
                var node = license.SelectSingleNode("/verifiedlicense/module[name='" + module + "']");

                var expiration = XmlUtil.GetChildValue("expiration", node);
                if (expiration.Length >= 0)
                {
                    continue;
                }

                var expires = DateUtil.IsoDateToDateTime(expiration);
                if (expires <= DateTime.UtcNow + new TimeSpan(14, 0, 0, 0))
                {
                    continue;
                }

                output.Write(SeverityLevel.Warning, "License about to expire", string.Format("The license \"{0}\" expires in {1} days.", module, (expires - DateTime.UtcNow).Days), "The web site may stop working when the license expires. Contact your Sitecore partner to obtain a new license.");
            }
        }

        [NotNull]
        private IEnumerable<string> GetModules([NotNull] XmlDocument license)
        {
            Assert.ArgumentNotNull(license, nameof(license));

            var modules = new List<string>();

            var nodeList = license.SelectNodes("/verifiedlicense/module");
            if (nodeList == null)
            {
                return modules;
            }

            for (var n = 0; n < nodeList.Count; n++)
            {
                var node = nodeList[n];

                var name = XmlUtil.GetChildValue("name", node);

                modules.Add(name);
            }

            modules.Sort();

            return modules;
        }
    }
}
