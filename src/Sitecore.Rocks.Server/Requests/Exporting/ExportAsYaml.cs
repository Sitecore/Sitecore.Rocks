// © 2015-2017 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Rocks.Server.IO;

namespace Sitecore.Rocks.Server.Requests.Exporting
{
    public class ExportAsYaml
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id)
        {
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id);
            Debug.Assert(item != null, "Item \"" + id + "\" not found.");

            var fileName = FileUtil.MapPath(TempFolder.GetFilename("export.yaml"));
            var languages = item.Database.Languages;
            var templates = TemplateManager.GetTemplates(database).Values;
            var duplicateTemplateNames = templates.Where(t => templates.Any(d => d != t && string.Equals(d.Name, t.Name, StringComparison.OrdinalIgnoreCase))).Select(t => t.Name);

            using (var writer = new StreamWriter(fileName))
            {
                Export(new YamlTextWriter(writer), item, languages, duplicateTemplateNames, true);
            }

            return fileName;
        }

        private void Export(YamlTextWriter output, Item item, Language[] languages, IEnumerable<string> duplicateTemplateNames, bool isRoot)
        {
            if (item.TemplateID == TemplateIDs.Template)
            {
                var template = TemplateManager.GetTemplate(item.ID, item.Database);
                if (template != null)
                {
                    ExportTemplate(output, template);
                }

                return;
            }

            output.WriteStartElement(item.TemplateName, item.Name);
            if (duplicateTemplateNames.Contains(item.TemplateName))
            {
                output.WriteAttributeString("TemplateName", item.Database.GetItem(item.TemplateID).Paths.Path);
            }

            output.WriteAttributeString("Id", item.ID.ToString());

            if (isRoot)
            {
                output.WriteAttributeString("ItemPath", item.Paths.Path);
                output.WriteAttributeString("Database", item.Database.Name);
            }

            item.Fields.ReadAll();

            var versionedItems = item.Versions.GetVersions(true);
            var versions = new List<Tuple<Language, List<Field>, List<Tuple<int, List<Field>>>>>();
            var sharedFields = item.Fields.Where(f => !f.Name.StartsWith("__") && f.Shared && !string.IsNullOrEmpty(f.Value)).ToArray();

            foreach (var language in languages)
            {
                var unversionedItem = versionedItems.FirstOrDefault(i => i.Language == language);
                if (unversionedItem == null)
                {
                    continue;
                }

                unversionedItem.Fields.ReadAll();

                var unversionedFields = unversionedItem.Fields.Where(f => !f.Name.StartsWith("__") && !f.Shared && f.Unversioned && !string.IsNullOrEmpty(f.Value)).ToList();

                var versionedFields = new List<Tuple<int, List<Field>>>();
                foreach (var versionedItem in versionedItems.Where(i => i.Language == language))
                {
                    versionedItem.Fields.ReadAll();

                    var flds = versionedItem.Fields.Where(f => !f.Name.StartsWith("__") && !f.Shared && !f.Unversioned && !string.IsNullOrEmpty(f.Value)).ToList();
                    if (flds.Any())
                    {
                        var tuple = new Tuple<int, List<Field>>(versionedItem.Version.Number, flds);
                        versionedFields.Add(tuple);
                    }
                }

                if (unversionedFields.Any() || versionedFields.Any())
                {
                    versions.Add(new Tuple<Language, List<Field>, List<Tuple<int, List<Field>>>>(language, unversionedFields, versionedFields));
                }
            }

            if (versions.Any() || sharedFields.Any())
            {
                output.WriteStartElement("Fields");

                foreach (var field in sharedFields.OrderBy(f => f.Name))
                {
                    var value = GetValue(field);
                    output.WriteAttributeStringIf(field.Name, value);
                }

                if (versions.Any())
                {
                    foreach (var language in versions.OrderBy(l => l.Item1.Name))
                    {
                        output.WriteStartElement(language.Item1.Name);

                        foreach (var field in language.Item2.OrderBy(f => f.Name))
                        {
                            var value = GetValue(field);
                            output.WriteAttributeStringIf(field.Name, value);
                        }

                        foreach (var versionedItem in language.Item3.OrderBy(t => t.Item1))
                        {
                            output.WriteStartElement(versionedItem.Item1.ToString());

                            foreach (var field in versionedItem.Item2.OrderBy(f => f.Name))
                            {
                                var value = GetValue(field);
                                output.WriteAttributeStringIf(field.Name, value);
                            }

                            output.WriteEndElement();
                        }

                        output.WriteEndElement();
                    }

                }

                output.WriteEndElement();
            }

            if (item.Children.Any())
            {
                output.WriteStartElement("Items");

                foreach (Item child in item.Children)
                {
                    Export(output, child, languages, duplicateTemplateNames, false);
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }

        [NotNull]
        private string GetValue(Field field)
        {
            switch (field.Type.ToLowerInvariant())
            {
                case "checkbox":
                    return field.Value == "1" ? "true" : "false";
                case "image":
                    if (field.Value == "<media />" || field.Value == "<media/>")
                    {
                        return string.Empty;
                    }
                    var imageField = new ImageField(field);
                    var imageItem = imageField.MediaItem;
                    return imageItem != null ? imageItem.Paths.Path : field.Value;
                case "link":
                    if (field.Value == "<link />" || field.Value == "<link/>")
                    {
                        return string.Empty;
                    }

                    var linkField = new LinkField(field);
                    var linkItem = linkField.TargetItem;
                    return linkItem != null ? linkItem.Paths.Path : field.Value;
                default:
                    return field.Value;

            }
        }

        private void ExportTemplate(YamlTextWriter output, Template template)
        {
            // todo: __Standard Values

            output.WriteStartElement("Template", template.Name);
            output.WriteAttributeString("Id", template.ID.ToString());
            output.WriteAttributeStringIf("Icon", template.Icon);
            output.WriteAttributeStringIf("BaseTemplates", string.Join("|", template.BaseIDs.Select(id => id.ToString())));

            foreach (var templateSection in template.GetSections())
            {
                if (templateSection.Name == "Section")
                {
                    output.WriteStartElement("Section", templateSection.Name);
                }
                else
                {
                    output.WriteStartElement(templateSection.Name);
                }
                output.WriteAttributeString("Id", templateSection.ID.ToString());

                foreach (var templateSectionField in templateSection.GetFields())
                {
                    if (templateSectionField.Name == "Field")
                    {
                        output.WriteStartElement("Field", templateSectionField.Name);
                    }
                    else
                    {
                        output.WriteStartElement(templateSectionField.Name);
                    }

                    output.WriteAttributeString("Id", templateSectionField.ID.ToString());
                    output.WriteAttributeStringIf("Type", templateSectionField.Type);
                    output.WriteAttributeStringIf("Source", templateSectionField.Source);
                    output.WriteAttributeString("SortOrder", templateSectionField.Sortorder.ToString());
                    output.WriteAttributeStringIf("Icon", templateSectionField.Icon);

                    if (templateSectionField.IsShared)
                    {
                        output.WriteAttributeStringIf("Sharing", "Shared");
                    }
                    else if (templateSectionField.IsUnversioned)
                    {
                        output.WriteAttributeStringIf("Sharing", "Unversioned");
                    }

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }
    }
}
