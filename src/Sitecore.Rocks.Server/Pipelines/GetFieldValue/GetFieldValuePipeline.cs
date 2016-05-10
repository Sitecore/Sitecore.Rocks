// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.GetFieldValue
{
    public class GetFieldValuePipeline : Pipeline<GetFieldValuePipeline>
    {
        public Field Field { get; private set; }

        public string Value { get; set; }

        [NotNull]
        public GetFieldValuePipeline WithParameters([NotNull] Item item, [NotNull] string fieldName)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));

            var field = item.Fields[fieldName];

            item.Fields.EnsureField(field);

            return WithParameters(field);
        }

        [NotNull]
        public GetFieldValuePipeline WithParameters([NotNull] Field field)
        {
            Assert.ArgumentNotNull(field, nameof(field));

            Field = field;

            Start();

            return this;
        }
    }
}
