// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.XmlNodeExtensions;

namespace Sitecore.Rocks.Data.DataServices.WebServiceMethods
{
    public static class GetItemHeader
    {
        [NotNull]
        public static ItemHeader Call([NotNull] XmlNode item, [NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var templateId = ItemId.Empty;
            var standardValuesField = ItemId.Empty;
            var standardValuesId = ItemId.Empty;
            var sortorder = 0;

            var name = item.InnerText;
            var path = item.GetAttributeValue("path");
            var templateName = item.GetAttributeValue("templatename");
            var category = item.GetAttributeValue("category");
            var updatedBy = item.GetAttributeValue("updatedby");
            var updated = DateTimeExtensions.FromIso(item.GetAttributeValue("updated"));
            var locked = item.GetAttributeValue("locked");
            var ownership = item.GetAttributeValue("owner");
            var parentName = item.GetAttributeValue("parentname");
            var isClone = item.GetAttributeValue("clone") == "1";
            var source = item.GetAttributeValue("source");
            var serializationStatus = SerializationStatus.NotSerialized;

            var value = item.GetAttributeValue("templateid");
            if (!string.IsNullOrEmpty(value))
            {
                templateId = new ItemId(new Guid(value));
            }

            value = item.GetAttributeValue("standardvaluesid");
            if (!string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    standardValuesId = new ItemId(new Guid(value));
                }
                else
                {
                    standardValuesId = ItemId.Empty;
                }
            }

            value = item.GetAttributeValue("standardvaluesfield");
            if (!string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    standardValuesField = new ItemId(new Guid(value));
                }
                else
                {
                    standardValuesField = ItemId.Empty;
                }
            }

            value = item.GetAttributeValue("sortorder");
            if (!string.IsNullOrEmpty(value))
            {
                int.TryParse(value, out sortorder);
            }

            value = item.GetAttributeValue("serializationstatus");
            if (!string.IsNullOrEmpty(value))
            {
                switch (value)
                {
                    case "0":
                        serializationStatus = SerializationStatus.NotSerialized;
                        break;
                    case "1":
                        serializationStatus = SerializationStatus.Serialized;
                        break;
                    case "2":
                        serializationStatus = SerializationStatus.Modified;
                        break;
                    case "3":
                        serializationStatus = SerializationStatus.Error;
                        break;
                }
            }

            var dbUri = databaseUri;

            var databaseName = item.GetAttributeValue("database");
            if (!string.IsNullOrEmpty(databaseName) && databaseName != databaseUri.DatabaseName.Name)
            {
                dbUri = new DatabaseUri(databaseUri.Site, new DatabaseName(databaseName));
            }

            var result = new ItemHeader
            {
                HasChildren = item.GetAttributeValue("haschildren") == @"1",
                Name = name,
                TemplateId = templateId,
                StandardValuesId = standardValuesId,
                StandardValuesField = standardValuesField,
                Icon = new Icon(dbUri.Site, item.GetAttributeValue("icon")),
                ItemUri = new ItemUri(dbUri, new ItemId(new Guid(item.GetAttributeValue("id")))),
                TemplateName = templateName,
                Path = path,
                Category = category,
                SortOrder = sortorder,
                UpdatedBy = updatedBy,
                Updated = updated,
                Locked = locked,
                Ownership = ownership,
                ParentName = parentName,
                /* IsClone = isClone, */
                Source = source,
                SerializationStatus = serializationStatus
            };

            if (item.Attributes == null)
            {
                return result;
            }

            foreach (XmlAttribute attribute in item.Attributes)
            {
                if (attribute.LocalName.StartsWith("ex."))
                {
                    result.SetData(attribute.LocalName, attribute.Value);
                }
            }

            return result;
        }
    }
}
