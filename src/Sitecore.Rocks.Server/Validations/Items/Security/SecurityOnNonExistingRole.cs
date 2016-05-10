// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;

namespace Sitecore.Rocks.Server.Validations.Items.Security
{
    [Validation("Security assigned to non-existing role", "Security")]
    public class SecurityOnNonExistingRole : ItemValidation
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
                if (rule.Account.AccountType != AccountType.Role)
                {
                    continue;
                }

                if (Role.Exists(rule.Account.Name))
                {
                    return;
                }

                var role = Role.FromName(rule.Account.Name);
                if (role != null)
                {
                    if (role.IsEveryone)
                    {
                        return;
                    }

                    if (role.IsGlobal)
                    {
                        return;
                    }
                }

                output.Write(SeverityLevel.Suggestion, "Security set on non-existing role", string.Format("\"{0}\" is assigned to the non-existing role: {1}", rule.AccessRight.Title, rule.Account.DisplayName), "Remove the security assignment.", item);
                break;
            }
        }
    }
}
