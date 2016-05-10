// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Items.Fields
{
    public class GeneralLinkFieldWriter : FieldWriterBase
    {
        public GeneralLinkFieldWriter([NotNull] XmlTextWriter writer) : base(writer)
        {
            Assert.ArgumentNotNull(writer, nameof(writer));
        }

        public override void WriteField([NotNull] Item item, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(field, nameof(field));

            LinkField linkField = field;
            var targetItem = linkField.TargetItem;

            if (targetItem == null)
            {
                return;
            }

            Writer.WriteStartElement("link");
            Writer.WriteAttributeString("path", item.Paths.Path);
            Writer.WriteEndElement();
        }
    }
}
