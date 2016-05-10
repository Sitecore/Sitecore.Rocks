// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields.Notifications
{
    public class FieldDataNotSpecifiedNotification : NotificationBase
    {
        public FieldDataNotSpecifiedNotification() : this(true)
        {
        }

        public FieldDataNotSpecifiedNotification(bool hidden)
        {
            Initialize(Rocks.Resources.FieldDataNotSpecifiedNotification_Text, hidden);
        }

        public void Initialize([NotNull] Exception exception)
        {
            Assert.ArgumentNotNull(exception, nameof(exception));
        }
    }
}
