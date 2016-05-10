// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.ContentEditors.Fields.Notifications
{
    public class FieldOutdatedNotification : NotificationBase
    {
        public FieldOutdatedNotification() : this(true)
        {
        }

        public FieldOutdatedNotification(bool hidden)
        {
            Initialize(Rocks.Resources.FieldOutdatedNotification_Text, hidden);
        }
    }
}
