// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Requests.Rules;

namespace Sitecore.Rocks.Server.Requests.Items.Fields
{
    public class RuleSetFieldWriter : FieldWriterBase
    {
        public RuleSetFieldWriter([NotNull] XmlTextWriter writer) : base(writer)
        {
            Assert.ArgumentNotNull(writer, nameof(writer));
        }

        public override void WriteField([NotNull] Item item, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(field, nameof(field));

            var request = new GetConditionsAndActions();
            request.Write(Writer, item.Database, field.Source);
        }
    }
}
