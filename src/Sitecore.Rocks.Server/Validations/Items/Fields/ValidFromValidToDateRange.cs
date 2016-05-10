// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Fields
{
    [Validation("Valid From/Valid To dates", "Fields")]
    public class ValidFromValidToDateRange : DateRangeChecker
    {
        public ValidFromValidToDateRange()
        {
            Field1 = FieldIDs.ValidFrom.ToString();
            Field2 = FieldIDs.ValidTo.ToString();
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            if (item.Publishing.NeverPublish)
            {
                return;
            }

            base.Check(output, item);
        }

        protected override void WriteError(ValidationWriter output, Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.Write(SeverityLevel.Suggestion, "Valid From/Valid To dates", "The Valid From date is after the Valid To date.", "Change either the Valid From date or the Valid To date.", item);
        }
    }
}
