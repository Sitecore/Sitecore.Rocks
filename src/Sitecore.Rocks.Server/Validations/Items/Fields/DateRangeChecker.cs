// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Fields
{
    [Serializable]
    public abstract class DateRangeChecker : ItemValidation
    {
        protected string Field1 { get; set; }

        protected string Field2 { get; set; }

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

            var value1 = item[Field1];
            var value2 = item[Field2];

            if (string.IsNullOrEmpty(value1) || string.IsNullOrEmpty(value2))
            {
                return;
            }

            var date1 = DateUtil.IsoDateToDateTime(value1);
            var date2 = DateUtil.IsoDateToDateTime(value2);

            if (date1 <= date2)
            {
                return;
            }

            WriteError(output, item);
        }

        protected abstract void WriteError([NotNull] ValidationWriter output, [NotNull] Item item);
    }
}
