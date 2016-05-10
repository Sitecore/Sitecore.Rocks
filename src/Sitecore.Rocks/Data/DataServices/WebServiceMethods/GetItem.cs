// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XmlNodeExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.Data.DataServices.WebServiceMethods
{
    public static class GetItem
    {
        [NotNull]
        public static Item Call([NotNull] ItemVersionUri uri, [NotNull] XmlElement root)
        {
            Assert.ArgumentNotNull(uri, nameof(uri));
            Assert.ArgumentNotNull(root, nameof(root));

            var versionNumber = 0;
            var node = root.SelectSingleNode(@"/field");
            if (node != null)
            {
                if (!int.TryParse(node.GetAttributeValue("version"), out versionNumber))
                {
                    versionNumber = 0;
                }
            }

            var result = new Item
            {
                Fields = new List<Field>(),
                Uri = new ItemVersionUri(uri.ItemUri, uri.Language, new Version(versionNumber)),
                Source = root.GetAttributeValue("source")
            };

            var fields = root.SelectNodes(@"/field");
            if (fields == null)
            {
                return result;
            }

            var sections = new List<Section>();

            foreach (XmlNode child in fields)
            {
                int sortOrder;
                if (!int.TryParse(child.GetAttributeValue("sortorder"), out sortOrder))
                {
                    sortOrder = 0;
                }

                int sectionSortOrder;
                if (!int.TryParse(child.GetAttributeValue("sectionsortorder"), out sectionSortOrder))
                {
                    sectionSortOrder = 0;
                }

                var sectionExpandedByDefault = true;
                var isSectionExpanded = child.GetAttributeValue("sectionexpandedbydefault");
                if (!string.IsNullOrEmpty(isSectionExpanded))
                {
                    sectionExpandedByDefault = isSectionExpanded == "1";
                }

                Guid sectionItemId;
                if (!Guid.TryParse(child.GetAttributeValue("sectionid"), out sectionItemId))
                {
                    sectionItemId = Guid.Empty;
                }

                var sectionIcon = new Icon(uri.Site, child.GetAttributeValue("sectionicon"));

                var templateFieldId = ItemId.Empty;
                var templateFieldIdString = child.GetAttributeValue("templatefieldid");
                if (!string.IsNullOrEmpty(templateFieldIdString))
                {
                    templateFieldId = new ItemId(new Guid(templateFieldIdString));
                }

                var section = GetSection(sections, child.GetAttributeValue("section"), sectionIcon, sectionSortOrder, new ItemUri(uri.ItemUri.DatabaseUri, new ItemId(sectionItemId)), sectionExpandedByDefault);

                var value = string.Empty;
                var valueElement = child.SelectSingleNode(@"value");
                if (valueElement != null)
                {
                    value = valueElement.InnerText;
                }

                var displayData = string.Empty;
                var displayDataElement = child.SelectSingleNode(@"displaydata");
                if (displayDataElement != null)
                {
                    displayData = displayDataElement.OuterXml;
                }

                var lookup = string.Empty;
                var lookupElement = child.SelectSingleNode(@"lookup");
                if (lookupElement != null)
                {
                    lookup = lookupElement.OuterXml;
                }

                var field = new Field
                {
                    Name = child.GetAttributeValue("name"),
                    Title = child.GetAttributeValue("title"),
                    ToolTip = child.GetAttributeValue("tooltip"),
                    Type = child.GetAttributeValue("type"),
                    Source = child.GetAttributeValue("source"),
                    Section = section,
                    SortOrder = sortOrder,
                    Shared = child.GetAttributeValue("shared") == @"1",
                    IsDeclaringTemplate = child.GetAttributeValue("isdeclaringtemplate") == @"1",
                    Unversioned = child.GetAttributeValue("unversioned") == @"1",
                    StandardValue = child.GetAttributeValue("standardvalue") == @"1",
                    IsBlob = child.GetAttributeValue("isblob") == @"1",
                    TemplateFieldId = templateFieldId,
                    Value = value,
                    DisplayData = displayData,
                    Lookup = lookup,
                    HasValue = true
                };

                var itemUri = new ItemUri(uri.ItemUri.DatabaseUri, new ItemId(new Guid(child.GetAttributeValue("itemid"))));
                var itemVersionUri = new ItemVersionUri(itemUri, new Language(child.GetAttributeValue("language")), new Version(int.Parse(child.GetAttributeValue("version"))));
                var fieldUri = new FieldUri(itemVersionUri, new FieldId(new Guid(child.GetAttributeValue("fieldid"))));

                field.FieldUris.Add(fieldUri);

                GetValueItems(child, field);
                GetRootItem(child, field);

                field.OriginalValue = field.Value;

                result.Fields.Add(field);
            }

            result.Fields.Sort(new FieldComparer());

            var pathItem = root.SelectSingleNode(@"/path/item");
            result.Name = pathItem != null ? pathItem.GetAttributeValue("name") : Resources.GetItem_Call__Unknown_;

            GetItemVersions(root, result);
            GetItemLanguages(root, result);
            GetItemPath(root, result, uri.ItemUri.Site, uri.ItemUri.DatabaseName.Name);
            GetItemTemplate(root, result);
            GetStandardValues(root, result);
            GetBaseTemplates(root, result);
            GetItemIcon(uri.Site, root, result);
            GetItemWarnings(root, uri.ItemUri.Site, result);
            GetBreadcrumb(root, result);

            foreach (XmlAttribute attribute in root.Attributes)
            {
                if (attribute.LocalName.StartsWith("ex."))
                {
                    result.SetData(attribute.LocalName, attribute.Value);
                }
            }

            return result;
        }

        private static void GetBaseTemplates([NotNull] XmlElement root, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(item, nameof(item));

            item.BaseTemplates = new List<ItemHeader>();

            var templates = root.SelectNodes(@"/basetemplates/template");
            if (templates == null)
            {
                return;
            }

            foreach (XmlNode template in templates)
            {
                item.BaseTemplates.Add(GetItemHeader.Call(template, item.Uri.ItemUri.DatabaseUri));
            }
        }

        private static void GetBreadcrumb([NotNull] XmlElement root, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(item, nameof(item));

            item.Breadcrumb = string.Empty;

            var breadcrumb = root.SelectSingleNode(@"/breadcrumb");
            if (breadcrumb == null)
            {
                return;
            }

            item.Breadcrumb = breadcrumb.OuterXml;
        }

        private static void GetItemIcon([NotNull] Site site, [NotNull] XmlElement root, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(item, nameof(item));

            var icon = root.SelectSingleNode(@"/icon");
            if (icon == null)
            {
                item.Icon = new Icon("Resources/32x32/cube_blue.png");
                return;
            }

            item.Icon = new Icon(site, icon.GetAttributeValue("small"));
        }

        private static void GetItemLanguages([NotNull] XmlElement root, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(item, nameof(item));

            var languages = root.SelectNodes(@"/languages/language");
            if (languages == null)
            {
                return;
            }

            foreach (XmlNode language in languages)
            {
                var attributes = language.Attributes;
                if (attributes == null)
                {
                    continue;
                }

                var attribute = attributes[@"name"];
                if (attribute == null)
                {
                    continue;
                }

                item.Languages.Add(attribute.Value);
            }
        }

        private static void GetItemPath([NotNull] XmlElement root, [NotNull] Item result, [NotNull] Site site, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(result, nameof(result));
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var items = root.SelectNodes(@"/path/item");
            if (items == null)
            {
                return;
            }

            foreach (XmlNode item in items)
            {
                var itemId = new Guid(item.GetAttributeValue("id"));

                var itemPath = new ItemPath
                {
                    Name = item.GetAttributeValue("name"),
                    ItemUri = new ItemUri(new DatabaseUri(site, new DatabaseName(databaseName)), new ItemId(itemId))
                };

                result.Path.Add(itemPath);
            }
        }

        private static void GetItemTemplate([NotNull] XmlElement root, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(item, nameof(item));

            var template = root.SelectSingleNode(@"/template");
            if (template == null)
            {
                item.TemplateId = ItemId.Empty;
                item.TemplateName = string.Empty;
                return;
            }

            item.TemplateId = new ItemId(new Guid(template.GetAttributeValue("templateid")));
            item.TemplateName = template.InnerText;
        }

        private static void GetItemVersions([NotNull] XmlElement root, [NotNull] Item result)
        {
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(result, nameof(result));

            var versions = root.SelectNodes(@"/versions/version");
            if (versions == null)
            {
                return;
            }

            foreach (XmlNode version in versions)
            {
                int number;
                if (!int.TryParse(version.GetAttributeValue("number"), out number))
                {
                    continue;
                }

                result.Versions.Add(number);
            }
        }

        private static void GetItemWarnings([NotNull] XmlElement root, [NotNull] Site site, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(item, nameof(item));

            var warnings = root.SelectNodes(@"/warnings/warning");
            if (warnings == null)
            {
                return;
            }

            var server = site.GetHost();

            foreach (XmlNode warning in warnings)
            {
                var w = new Warning
                {
                    Title = warning.GetAttributeValue("title"),
                    Text = warning.InnerText,
                    Icon = server + warning.GetAttributeValue("icon")
                };

                item.Warnings.Add(w);
            }
        }

        private static void GetRootItem([NotNull] XmlNode child, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(child, nameof(child));
            Assert.ArgumentNotNull(field, nameof(field));

            var root = child.SelectSingleNode(@"root/item");
            if (root == null)
            {
                return;
            }

            var databaseUri = field.FieldUris.First().ItemVersionUri.ItemUri.DatabaseUri;

            var urlString = new UrlString(field.Source);

            var databaseName = urlString[@"databasename"];
            if (!string.IsNullOrEmpty(databaseName))
            {
                databaseUri = new DatabaseUri(databaseUri.Site, new DatabaseName(databaseName));
            }

            field.Root = GetItemHeader.Call(root, databaseUri);
        }

        [NotNull]
        private static Section GetSection([NotNull] List<Section> sections, [NotNull] string name, [NotNull] Icon icon, int sortOrder, [NotNull] ItemUri itemUri, bool sectionExpandedByDefault)
        {
            Debug.ArgumentNotNull(sections, nameof(sections));
            Debug.ArgumentNotNull(name, nameof(name));
            Debug.ArgumentNotNull(icon, nameof(icon));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(sections, nameof(sections));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            foreach (var section in sections)
            {
                if (section.Name == name && section.SortOrder == sortOrder && section.ItemUri == itemUri)
                {
                    return section;
                }
            }

            var result = new Section
            {
                Name = name,
                Icon = icon,
                SortOrder = sortOrder,
                ExpandedByDefault = sectionExpandedByDefault,
                ItemUri = itemUri
            };

            sections.Add(result);

            return result;
        }

        private static void GetStandardValues([NotNull] XmlElement root, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(item, nameof(item));

            var standardValues = root.SelectSingleNode(@"/standardvalues");
            if (standardValues == null)
            {
                item.StandardValuesId = ItemId.Empty;
                return;
            }

            var value = standardValues.GetAttributeValue("standardvaluesid");

            if (string.IsNullOrEmpty(value))
            {
                item.StandardValuesId = ItemId.Empty;
            }
            else
            {
                item.StandardValuesId = new ItemId(new Guid(value));
            }
        }

        private static void GetValueItems([NotNull] XmlNode child, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(child, nameof(child));
            Assert.ArgumentNotNull(field, nameof(field));

            var valueItems = child.SelectNodes(@"lookup/item");
            if (valueItems == null || valueItems.Count == 0)
            {
                return;
            }

            foreach (XmlNode valueItem in valueItems)
            {
                var name = valueItem.GetAttributeValue("name");
                var value = valueItem.GetAttributeValue("value");

                field.ValueItems.Add(new ValueItem(name, value));
            }
        }
    }
}
