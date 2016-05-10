// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.Pipelines.GetContentEditorWarnings;
using Sitecore.Resources;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;
using Sitecore.Rocks.Server.Pipelines.WriteItemHeader;
using Sitecore.Rocks.Server.Requests.Items.Fields;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls.Data;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class GetItemFields
    {
        [NotNull]
        public string Execute([NotNull] string id, [NotNull] string language, [NotNull] string version, bool allFields, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(language, nameof(language));
            Assert.ArgumentNotNull(version, nameof(version));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id, Language.Parse(language), Data.Version.Parse(version));
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("item");
            try
            {
                WriteCloneInfo(output, item);
            }
            catch (MissingMethodException)
            {
                output.WriteAttributeString("clone", "0");
            }

            WriteItemHeaderPipeline.Run().WithParameters(output, item);

            GetFields(output, item, allFields);
            GetItemVersions(output, item);
            GetItemLanguages(output, item);
            GetItemPath(output, item);
            GetItemTemplate(output, item);
            GetStandardValues(output, item);
            GetItemBaseTemplates(output, item);
            GetItemIcon(output, item);
            GetContentEditorWarnings(output, item);
            GetBreadcrumb(output, item);

            output.WriteEndElement();

            return writer.ToString();
        }

        public static void GetBreadcrumbItems([NotNull] XmlTextWriter output, [NotNull] Item selectedItem, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(selectedItem, nameof(selectedItem));
            Assert.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("item");
            output.WriteAttributeString("id", item.ID.ToString());
            output.WriteAttributeString("name", item.Name);
            output.WriteAttributeString("icon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));

            foreach (Item child in item.Children)
            {
                if (child.Axes.IsAncestorOf(selectedItem))
                {
                    GetBreadcrumbItems(output, selectedItem, child);
                }
                else
                {
                    output.WriteStartElement("item");
                    output.WriteAttributeString("id", child.ID.ToString());
                    output.WriteAttributeString("name", child.Name);
                    output.WriteAttributeString("icon", Images.GetThemedImageSource(child.Appearance.Icon, ImageDimension.id16x16));
                    output.WriteEndElement();
                }
            }

            output.WriteEndElement();
        }

        private void GetBreadcrumb([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("breadcrumb");

            GetBreadcrumbItems(output, item, item.Database.GetRootItem());

            output.WriteEndElement();
        }

        private void GetContentEditorWarnings([NotNull] XmlWriter output, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            var getContentEditorWarningsArgs = new GetContentEditorWarningsArgs(item)
            {
                ShowInputBoxes = true,
                HasSections = true
            };

            try
            {
                CorePipeline.Run("getContentEditorWarnings", getContentEditorWarningsArgs);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load content editor warnings", ex, this);
                return;
            }

            if (getContentEditorWarningsArgs.Warnings.Count <= 0)
            {
                return;
            }

            output.WriteStartElement("warnings");

            foreach (var warning in getContentEditorWarningsArgs.Warnings)
            {
                if (string.IsNullOrEmpty(warning.Text))
                {
                    continue;
                }

                output.WriteStartElement("warning");

                output.WriteAttributeString("title", warning.Title);
                output.WriteAttributeString("isexclusive", warning.IsExclusive ? "1" : "0");
                output.WriteAttributeString("isfullscreen", warning.IsFullscreen ? "1" : "0");
                output.WriteAttributeString("hidefields", warning.HideFields ? "1" : "0");
                output.WriteAttributeString("icon", Images.GetThemedImageSource(warning.Icon, ImageDimension.id16x16));

                output.WriteValue(warning.Text);

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void GetFieldLookups([NotNull] XmlTextWriter output, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(field, nameof(field));

            var isValueLookup = false;
            var hasSubItems = false;

            IEnumerable<Item> items = null;

            try
            {
                switch (field.Type.ToLowerInvariant())
                {
                    case "lookup":
                    case "droplink":
                    case "multilist":
                    case "checklist":
                    case "name lookup value list":
                        if (field.Item != null && !string.IsNullOrEmpty(field.Source))
                        {
                            items = LookupSources.GetItems(field.Item, field.Source);
                        }

                        break;

                    case "valuelookup":
                    case "droplist":
                        if (field.Item != null && !string.IsNullOrEmpty(field.Source))
                        {
                            items = LookupSources.GetItems(field.Item, field.Source);
                            isValueLookup = true;
                        }

                        break;

                    case "tree list":
                    case "treelist":
                    case "treelistex":
                        MultilistField m = field;
                        items = m.GetItems();
                        break;

                    case "grouped droplist":
                        if (field.Item != null && !string.IsNullOrEmpty(field.Source))
                        {
                            items = LookupSources.GetItems(field.Item, field.Source);
                            hasSubItems = true;
                            isValueLookup = true;
                        }

                        break;

                    case "grouped droplink":
                        if (field.Item != null && !string.IsNullOrEmpty(field.Source))
                        {
                            items = LookupSources.GetItems(field.Item, field.Source);
                            hasSubItems = true;
                        }

                        break;

                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get Lookup source", ex);
                items = null;
            }

            if (items == null)
            {
                return;
            }

            output.WriteStartElement("lookup");

            foreach (var i in items)
            {
                var value = isValueLookup ? i.Name : i.ID.ToString();

                output.WriteStartElement("item");

                output.WriteAttributeString("value", value);
                output.WriteAttributeString("name", i.Name);

                if (hasSubItems)
                {
                    foreach (Item child in i.Children)
                    {
                        if (child == null)
                        {
                            continue;
                        }

                        value = isValueLookup ? child.Name : child.ID.ToString();

                        output.WriteStartElement("item");

                        output.WriteAttributeString("value", value);
                        output.WriteAttributeString("name", child.Name);

                        output.WriteEndElement();
                    }
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void GetFieldRoot([NotNull] XmlTextWriter output, [NotNull] Field field)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(field, nameof(field));

            var type = field.Type.ToLowerInvariant();
            if (type != "treelist" && type != "treelistex" && type != "tree list")
            {
                return;
            }

            var database = field.Database;
            var source = field.Source;
            string path;

            if (ID.IsID(source))
            {
                path = source;
            }
            else if (source.StartsWith("/"))
            {
                path = source;
            }
            else
            {
                path = StringUtil.ExtractParameter("DataSource", source).Trim().ToLower();
                if (string.IsNullOrEmpty(path))
                {
                    path = "/sitecore";
                }

                var databaseName = StringUtil.ExtractParameter("database", source).Trim().ToLower();
                if (string.IsNullOrEmpty(databaseName))
                {
                    databaseName = StringUtil.ExtractParameter("databasename", source).Trim().ToLower();
                }

                if (!string.IsNullOrEmpty(databaseName))
                {
                    database = Factory.GetDatabase(databaseName);
                }
            }

            var root = database.GetItem(path);
            if (root == null)
            {
                return;
            }

            output.WriteStartElement("root");
            output.WriteItemHeader(root, string.Empty);
            output.WriteEndElement();
        }

        private void GetFields([NotNull] XmlTextWriter output, [NotNull] Item item, bool allFields)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var fields = item.Fields;

            if (allFields)
            {
                fields.ReadAll();
            }

            fields.Sort();

            var sections = new Dictionary<string, SectionDefinition>();

            foreach (Field field in fields)
            {
                if (field.Name.Length <= 0)
                {
                    continue;
                }

                var templateField = field.GetTemplateField();

                SectionDefinition section;
                if (!sections.TryGetValue(field.Section, out section))
                {
                    section = new SectionDefinition();
                    sections[field.Section] = section;

                    var sectionItem = item.Database.GetItem(templateField.Section.ID);

                    section.SectionSortOrder = field.SectionSortorder.ToString();
                    section.ExpandByDefault = sectionItem["Collapsed By Default"] == "1" ? "0" : "1";
                }

                var isOwnField = templateField.Template.ID == item.TemplateID;

                output.WriteStartElement("field");

                output.WriteAttributeString("itemid", item.ID.ToString());
                output.WriteAttributeString("language", item.Language.ToString());
                output.WriteAttributeString("version", item.Version.ToString());

                output.WriteAttributeString("fieldid", field.ID.ToString());
                output.WriteAttributeString("templatefieldid", templateField.ID.ToString());
                output.WriteAttributeString("name", field.Name);
                output.WriteAttributeString("title", field.Title);
                output.WriteAttributeString("type", field.Type);
                output.WriteAttributeString("source", field.Source);
                output.WriteAttributeString("section", field.Section);
                output.WriteAttributeString("tooltip", field.ToolTip);
                output.WriteAttributeString("sortorder", field.Sortorder.ToString());
                output.WriteAttributeString("sectionsortorder", section.SectionSortOrder);
                output.WriteAttributeString("sectionexpandedbydefault", section.ExpandByDefault);
                output.WriteAttributeString("sectionid", templateField.Section.ID.ToString());
                output.WriteAttributeString("sectionicon", Images.GetThemedImageSource(templateField.Section.Icon, ImageDimension.id16x16));
                output.WriteAttributeString("isdeclaringtemplate", isOwnField ? "1" : "0");

                output.WriteAttributeString("shared", field.Shared ? "1" : "0");
                output.WriteAttributeString("unversioned", field.Unversioned ? "1" : "0");
                output.WriteAttributeString("standardvalue", field.ContainsStandardValue ? "1" : "0");
                output.WriteAttributeString("isblob", field.IsBlobField ? "1" : "0");
                output.WriteAttributeString("iscloned", field.InheritsValueFromOtherItem ? "1" : "0");

                WriteDisplayDataIfApplicable(output, item, field);

                output.WriteStartElement("value");
                output.WriteValue(GetFieldValuePipeline.Run().WithParameters(field).Value ?? string.Empty);
                output.WriteEndElement();

                GetFieldLookups(output, field);
                GetFieldRoot(output, field);

                output.WriteEndElement();
            }
        }

        private void GetItemBaseTemplates([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var template = item.Template;
            if (template == null)
            {
                return;
            }

            output.WriteStartElement("basetemplates");

            foreach (var baseTemplate in template.BaseTemplates)
            {
                output.WriteStartElement("template");

                output.WriteAttributeString("id", baseTemplate.ID.ToString());
                output.WriteAttributeString("icon", Images.GetThemedImageSource(baseTemplate.Icon, ImageDimension.id16x16));
                output.WriteAttributeString("haschildren", "0");
                output.WriteAttributeString("templateid", baseTemplate.ID.ToString());
                output.WriteAttributeString("category", string.Empty);
                output.WriteAttributeString("templatename", baseTemplate.Name);
                output.WriteAttributeString("path", baseTemplate.InnerItem.Paths.Path);

                output.WriteValue(baseTemplate.Name);

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void GetItemIcon([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("icon");

            output.WriteAttributeString("small", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));
            output.WriteAttributeString("large", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id32x32));

            output.WriteEndElement();
        }

        private void GetItemLanguages([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("languages");

            foreach (var lang in item.Database.Languages)
            {
                output.WriteStartElement("language");

                output.WriteAttributeString("name", lang.Name);
                output.WriteAttributeString("cultureinfo", lang.CultureInfo.Name);

                output.WriteValue(lang.CultureInfo.DisplayName);

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void GetItemPath([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("path");

            var current = item;
            while (current != null)
            {
                output.WriteStartElement("item");

                output.WriteAttributeString("id", current.ID.ToString());
                output.WriteAttributeString("name", current.Name);
                output.WriteAttributeString("displayname", current.DisplayName);

                output.WriteEndElement();

                current = current.Parent;
            }

            output.WriteEndElement();
        }

        private void GetItemTemplate([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("template");

            output.WriteAttributeString("templateid", item.TemplateID.ToString());

            output.WriteValue(item.TemplateName);

            output.WriteEndElement();
        }

        private void GetItemVersions([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            output.WriteStartElement("versions");

            foreach (var ver in item.Versions.GetVersionNumbers())
            {
                output.WriteStartElement("version");

                output.WriteAttributeString("number", ver.ToString());

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        private void GetStandardValues([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var value = item[FieldIDs.StandardValues];

            if (string.IsNullOrEmpty(value))
            {
                var template = item.Template;
                if (template != null)
                {
                    value = template.InnerItem[FieldIDs.StandardValues];
                }
            }

            if (!string.IsNullOrEmpty(value))
            {
                if (item.Database.GetItem(value) == null)
                {
                    value = string.Empty;
                }
            }

            output.WriteStartElement("standardvalues");

            output.WriteAttributeString("standardvaluesid", value);

            output.WriteEndElement();
        }

        private void WriteCloneInfo(XmlTextWriter output, Item item)
        {
            output.WriteAttributeString("clone", item.IsClone ? "1" : "0");
            if (item.SourceUri != null)
            {
                output.WriteAttributeString("source", item.SourceUri.DatabaseName + "|" + item.SourceUri.ItemID);
            }
        }

        private void WriteDisplayDataIfApplicable([NotNull] XmlWriter output, [NotNull] Item item, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(item, nameof(item));

            var writerFactory = new FieldWriterFactory(output);
            var writer = writerFactory.GetFieldWriter(field.Type);

            if (writer == null)
            {
                return;
            }

            output.WriteStartElement("displaydata");
            writer.WriteField(item, field);
            output.WriteEndElement();
        }

        public class SectionDefinition
        {
            public string ExpandByDefault { get; set; }

            public string SectionSortOrder { get; set; }
        }
    }
}
