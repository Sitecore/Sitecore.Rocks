// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Security.AccessControl;

namespace Sitecore.Rocks.Server.Validations.Items.Security
{
    [Validation("Explicit security deny", "Security")]
    public class ExplicitSecurityDenies : ItemValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            var rules = item.Security.GetAccessRules();

            foreach (var rule in rules)
            {
                if (rule.SecurityPermission != SecurityPermission.DenyAccess)
                {
                    continue;
                }

                if (rule.AccessRight.IsFieldRight)
                {
                    continue;
                }

                output.Write(SeverityLevel.Suggestion, "Explicit security deny", string.Format("\"{0}\" is explicitly denied which is not recommended.", rule.AccessRight.Title), "Use inheritance to restrict permission.", item);
                break;
            }
        }
    }
}
