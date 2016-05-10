// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;

namespace Sitecore.Rocks.Server.Validations.Items.Security
{
    [Validation("Locked by non-existing user", "Items")]
    public class LockedByNonExistingUserValidator : ItemValidation
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

            if (!item.Locking.IsLocked())
            {
                return;
            }

            var owner = item.Locking.GetOwner();
            if (string.IsNullOrEmpty(owner))
            {
                return;
            }

            if (User.Exists(owner))
            {
                return;
            }

            output.Write(SeverityLevel.Suggestion, "Locked by non-existing user", string.Format("The item is locked by the non-existing user: {0}", owner), "Unlock the item.", item);
        }
    }
}
