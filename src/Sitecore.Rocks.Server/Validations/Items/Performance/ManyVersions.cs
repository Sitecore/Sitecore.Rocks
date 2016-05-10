// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Performance
{
    [Validation("Item has many versions", "Performance", ExecutePerLanguage = true)]
    public class ManyVersions : ItemValidation
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

            var count = item.Versions.Count;
            if (count <= 10)
            {
                return;
            }

            output.Write(SeverityLevel.Warning, "Item has many version", string.Format("The item has {0} versions in the {1} language. Items with more than 10 version decrease performance.", count, item.Language.GetDisplayName()), "Remove some of the older versions.", item);
        }
    }
}
