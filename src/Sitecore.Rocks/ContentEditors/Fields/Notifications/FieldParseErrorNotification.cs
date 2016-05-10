// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields.Notifications
{
    public class FieldParseErrorNotification : ErrorBase
    {
        private readonly bool hidden;

        public FieldParseErrorNotification() : this(true)
        {
        }

        public FieldParseErrorNotification(bool hidden)
        {
            this.hidden = hidden;
        }

        public void Initialize([NotNull] Exception exception)
        {
            Assert.ArgumentNotNull(exception, nameof(exception));

            Initialize(Rocks.Resources.FieldParseErrorNotification_Text, exception, hidden);
        }
    }
}
