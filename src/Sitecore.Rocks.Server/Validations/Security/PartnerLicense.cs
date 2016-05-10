// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Diagnostics;
using Sitecore.SecurityModel.License;

namespace Sitecore.Rocks.Server.Validations.Security
{
    [Validation("Partner license", "Licensing")]
    public class PartnerLicense : Validation
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

            if (!License.Purpose.Contains("The Sitecore partner"))
            {
                return;
            }

            output.Write(SeverityLevel.Warning, "Partner license", string.Format("The active license belongs to the partner \"{0}\".", License.Licensee), "Obtain a valid customer license from your partner or Sitecore.");
        }
    }
}
