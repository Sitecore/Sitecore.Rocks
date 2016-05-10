// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Fields
{
    [Validation("Publish/Unpublish dates", "Fields")]
    public class PublishUnpublishDateRange : DateRangeChecker
    {
        public PublishUnpublishDateRange()
        {
            Field1 = FieldIDs.PublishDate.ToString();
            Field2 = FieldIDs.UnpublishDate.ToString();
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

            output.Write(SeverityLevel.Suggestion, "Publish/Unpublish dates", "The Publish date is after the Unpublish date.", "Change either the Publish date or the Unpublish date.", item);
        }
    }
}
