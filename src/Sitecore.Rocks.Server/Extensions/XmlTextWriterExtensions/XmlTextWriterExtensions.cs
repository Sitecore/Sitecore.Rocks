// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Resources;
using Sitecore.Rocks.Server.Pipelines.WriteItemHeader;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions
{
    public static class XmlTextWriterExtensions
    {
        public static void WriteItemHeader([NotNull] this XmlTextWriter output, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            WriteItemHeader(output, item, string.Empty);
        }

        public static void WriteItemHeader([NotNull] this XmlTextWriter output, [NotNull] Item item, [NotNull] string categoryName)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(categoryName, nameof(categoryName));

            var standardValuesId = item[FieldIDs.StandardValues];

            if (string.IsNullOrEmpty(standardValuesId))
            {
                var template = item.Template;
                if (template != null)
                {
                    standardValuesId = template.InnerItem[FieldIDs.StandardValues];
                }
            }

            if (!string.IsNullOrEmpty(standardValuesId))
            {
                if (item.Database.GetItem(standardValuesId) == null)
                {
                    standardValuesId = string.Empty;
                }
            }

            var parent = item.Parent;

            var icon = Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16);
            var n = icon.IndexOf("~", StringComparison.Ordinal);
            if (n > 0)
            {
                icon = icon.Mid(n);
            }

            output.WriteStartElement("item");

            output.WriteAttributeString("id", item.ID.ToString());
            output.WriteAttributeString("database", item.Database.Name);
            output.WriteAttributeString("icon", icon);
            output.WriteAttributeString("haschildren", item.HasChildren ? "1" : "0");
            output.WriteAttributeString("templateid", item.TemplateID.ToString());
            output.WriteAttributeString("category", categoryName);
            output.WriteAttributeString("templatename", item.TemplateName);
            output.WriteAttributeString("path", item.Paths.Path);
            output.WriteAttributeString("standardvaluesfield", item[FieldIDs.StandardValues]);
            output.WriteAttributeString("standardvaluesid", standardValuesId);
            output.WriteAttributeString("sortorder", item.Appearance.Sortorder.ToString());
            output.WriteAttributeString("updatedby", item.Statistics.UpdatedBy);
            output.WriteAttributeString("updated", DateUtil.ToIsoDate(item.Statistics.Updated));
            output.WriteAttributeString("locked", item.Locking.GetOwner());
            output.WriteAttributeString("owner", item.Security.GetOwner());
            output.WriteAttributeString("parentname", parent != null ? parent.Name : string.Empty);

            try
            {
                WriteCloneInfo(output, item);
            }
            catch (MissingMethodException)
            {
                output.WriteAttributeString("clone", "0");
            }

            WriteItemHeaderPipeline.Run().WithParameters(output, item);

            output.WriteValue(item.Name);

            output.WriteEndElement();
        }

        private static void WriteCloneInfo([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            output.WriteAttributeString("clone", item.IsClone ? "1" : "0");
            if (item.SourceUri != null)
            {
                output.WriteAttributeString("source", item.SourceUri.DatabaseName + "|" + item.SourceUri.ItemID);
            }
        }
    }
}
