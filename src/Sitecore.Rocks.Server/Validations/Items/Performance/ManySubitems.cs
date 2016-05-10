// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Collections;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Performance
{
    [Validation("Item has many subitems", "Performance")]
    public class ManySubitems : ItemValidation
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

            var count = item.GetChildren(ChildListOptions.IgnoreSecurity).Count;
            if (count > 100)
            {
                output.Write(SeverityLevel.Warning, "Item has many subitems", string.Format("The item has {0} subitems. Items with more than 100 subitems decrease performance.", count), "Change the structure of the tree to reduce the number of subitems.", item);
            }
        }
    }
}
