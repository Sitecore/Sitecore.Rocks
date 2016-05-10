// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Items with same display name on same level", "Items", ExecutePerLanguage = true)]
    public class ItemsWithSameDisplayName : ItemValidation
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

            var children = item.Children;

            for (var i0 = 0; i0 < children.Count; i0++)
            {
                var item0 = children[i0];
                if (string.IsNullOrEmpty(item0.DisplayName))
                {
                    return;
                }

                for (var i1 = i0 + 1; i1 < children.Count; i1++)
                {
                    var item1 = children[i1];

                    if (string.Compare(item0.DisplayName, item1.DisplayName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        output.Write(SeverityLevel.Suggestion, "Items with same display name on same level", string.Format("Two or more items have the same display name \"{0}\" on the same level.", item0.DisplayName), "Change the display name of one or more of the items.", item0);
                    }
                }
            }
        }
    }
}
