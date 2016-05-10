// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Fields
{
    [Validation("Reminder/Archive dates", "Fields")]
    public class ReminderArchiveDateRange : DateRangeChecker
    {
        public ReminderArchiveDateRange()
        {
            Field1 = FieldIDs.ReminderDate.ToString();
            Field2 = FieldIDs.ArchiveDate.ToString();
        }

        protected override void WriteError(ValidationWriter output, Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.Write(SeverityLevel.Suggestion, "Reminder/Archive dates", "The Reminder date is after the Archive date.", "Change either the Reminder date or the Archive date.", item);
        }
    }
}
