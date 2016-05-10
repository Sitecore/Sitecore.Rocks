// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Presentation
{
    [Validation("Layout on item overrides layout on standard values item", "Presentation")]
    public class LayoutOverridesStandardValues : ItemValidation
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

            if (StandardValuesManager.IsStandardValuesHolder(item))
            {
                return;
            }

            var layoutField = item.Fields[FieldIDs.LayoutField];
            if (layoutField == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(layoutField.Value))
            {
                return;
            }

            if (layoutField.ContainsStandardValue)
            {
                return;
            }

            if (layoutField.Value != layoutField.GetStandardValue())
            {
                output.Write(SeverityLevel.Suggestion, "Layout on item overrides layout on standard values item", "The layout on the item overrides the layout on the standard values item. Changing the layout on the standard values item will not change the layout on this item.", "Change the layout so it accommodates both layouts and store that layout on the standard values item.", item);
            }
        }
    }
}
