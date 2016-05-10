// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;

namespace Sitecore.Rocks.Server.Validations.Items.Security
{
    [Validation("Security assigned to user", "Security")]
    public class SecurityOnUser : ItemValidation
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

                if (rule.Account.Name == "sitecore\\Anonymous")
                {
                    continue;
                }

                if (rule.Account.Name == "$currentuser")
                {
                    continue;
                }

                output.Write(SeverityLevel.Suggestion, "Security assigned to user", string.Format("\"{0}\" is assigned to the user \"{1}\". It is recommended to assign security to roles only.", rule.AccessRight.Title, rule.Account.DisplayName), "Assign security to a role.", item);
                break;
            }
        }
    }
}
