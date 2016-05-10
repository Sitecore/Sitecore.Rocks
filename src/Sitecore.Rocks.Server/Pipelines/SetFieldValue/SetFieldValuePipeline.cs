// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.SetFieldValue
{
    public class SetFieldValuePipeline : Pipeline<SetFieldValuePipeline>
    {
        public Field Field { get; private set; }

        public string Value { get; set; }

        [NotNull]
        public SetFieldValuePipeline WithParameters([NotNull] Item item, [NotNull] string fieldName, [NotNull] string value)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(value, nameof(value));

            var field = item.Fields[fieldName];

            if (field == null)
            {
                item[fieldName] = string.Empty;
                field = item.Fields[fieldName];
            }

            return WithParameters(field, value);
        }

        [NotNull]
        public SetFieldValuePipeline WithParameters([NotNull] Field field, [NotNull] string value)
        {
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(value, nameof(value));

            Field = field;
            Value = value;

            Start();

            return this;
        }
    }
}
