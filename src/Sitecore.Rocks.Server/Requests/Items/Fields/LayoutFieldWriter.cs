// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Resources;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Items.Fields
{
    public class LayoutFieldWriter : FieldWriterBase
    {
        public LayoutFieldWriter([NotNull] XmlTextWriter writer) : base(writer)
        {
            Assert.ArgumentNotNull(writer, nameof(writer));
        }

        public override void WriteField([NotNull] Item item, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(field, nameof(field));

            var value = GetFieldValuePipeline.Run().WithParameters(field).Value;
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var layoutDefinition = LayoutDefinition.Parse(value);

            foreach (DeviceDefinition deviceDefinition in layoutDefinition.Devices)
            {
                if (string.IsNullOrEmpty(deviceDefinition.ID))
                {
                    continue;
                }

                var deviceItem = item.Database.GetItem(deviceDefinition.ID);
                if (deviceItem == null)
                {
                    continue;
                }

                var layoutDisplayName = string.Empty;
                var layoutIcon = string.Empty;

                var layoutId = deviceDefinition.Layout;
                if (!string.IsNullOrEmpty(layoutId))
                {
                    LayoutItem layoutItem = item.Database.GetItem(layoutId);
                    if (layoutItem != null)
                    {
                        layoutDisplayName = layoutItem.DisplayName;
                        layoutIcon = Images.GetThemedImageSource(layoutItem.Icon, ImageDimension.id16x16);
                    }
                }

                Writer.WriteStartElement(LayoutStruct.Node.Device);
                Writer.WriteAttributeString(LayoutStruct.Attribute.DisplayName, deviceItem.DisplayName);
                Writer.WriteAttributeString(LayoutStruct.Attribute.Icon, Images.GetThemedImageSource(deviceItem.Appearance.Icon, ImageDimension.id32x32));

                Writer.WriteStartElement(LayoutStruct.Node.Layout);
                Writer.WriteAttributeString(LayoutStruct.Attribute.DisplayName, layoutDisplayName);
                Writer.WriteAttributeString(LayoutStruct.Attribute.Icon, layoutIcon);

                var renderings = deviceDefinition.Renderings;
                var renderingItems = new RenderingRecords(item.Database);
                foreach (var rendering in from object renref in renderings
                    where renref != null && renref.GetType() == typeof(RenderingDefinition)
                    select renref as RenderingDefinition)
                {
                    var renderingItem = renderingItems[rendering.ItemID];
                    if (renderingItem == null)
                    {
                        continue;
                    }

                    Writer.WriteStartElement(LayoutStruct.Node.Rendering);

                    Writer.WriteAttributeString(LayoutStruct.Attribute.DisplayName, renderingItem.DisplayName);
                    Writer.WriteAttributeString(LayoutStruct.Attribute.Icon, Images.GetThemedImageSource(renderingItem.Icon, ImageDimension.id16x16));
                    Writer.WriteAttributeString(LayoutStruct.Attribute.ItemId, rendering.ItemID);

                    Writer.WriteEndElement();
                }

                Writer.WriteEndElement();
                Writer.WriteEndElement();
            }
        }

        private static class LayoutStruct
        {
            public static class Attribute
            {
                public const string DisplayName = @"dn";

                public const string Icon = @"i";

                public const string ItemId = @"id";
            }

            public static class Node
            {
                public const string Device = @"d";

                public const string Layout = @"l";

                public const string Rendering = @"r";
            }
        }
    }
}
