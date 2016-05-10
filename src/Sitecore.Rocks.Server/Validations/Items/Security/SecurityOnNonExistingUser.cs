// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;

namespace Sitecore.Rocks.Server.Validations.Items.Security
{
    [Validation("Security assigned to non-existing user", "Security")]
    public class SecurityOnNonExistingUser : ItemValidation
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
                if (rule.Account.AccountType != AccountType.User)
                {
                    continue;
                }

                if (User.Exists(rule.Account.Name))
                {
                    continue;
                }

                if (rule.Account.Name == "sitecore\\Anonymous")
                {
                    continue;
                }

                if (rule.Account.Name == "$currentuser")
                {
                    continue;
                }

                output.Write(SeverityLevel.Suggestion, "Security assigned to non-existing user", string.Format("\"{0}\" assigned to the non-existing user \"{1}\".", rule.AccessRight.Title, rule.Account.DisplayName), "Remove the security assignment.", item);
                break;
            }
        }
    }
}
