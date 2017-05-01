using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Rocks.Server.Extensions.JsonTextWriterExtensions;

namespace Sitecore.Rocks.Server.Requests.Exporting
{
    public class ExportAsJson
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

            var fileName = FileUtil.MapPath(TempFolder.GetFilename("export.json"));

            using (var writer = new StreamWriter(fileName))
            {
                using (var output = new JsonTextWriter(writer))
                {
                    output.Formatting = Formatting.Indented;

                    output.WriteStartObject();

                    Export(output, new[] { item }, true);

                    output.WriteEndObject();
                }
            }

            return fileName;
        }

        private void Export(JsonTextWriter output, IEnumerable<Item> items, bool isRoot)
        {
            var templateNames = items.Select(i => i.TemplateName).Distinct();

            foreach (var templateName in templateNames.OrderBy(n => n))
            {
                var itemsOfTemplate = items.Where(i => i.TemplateName == templateName);

                var isSingleItem = itemsOfTemplate.Count() == 1;
                if (isSingleItem)
                {
                    output.WritePropertyName(templateName);
                }
                else
                {
                    output.WriteStartArray(templateName);
                }

                foreach (var item in itemsOfTemplate.OrderBy(i => i.Appearance.Sortorder).ThenBy(i => i.Name))
                {
                    Export(output, item, isRoot);
                }

                if (!isSingleItem)
                {
                    output.WriteEndArray();
                }
            }
        }

        private void Export(JsonTextWriter output, Item item, bool isRoot)
        {
            item.Fields.ReadAll();

            var versionedItems = item.Versions.GetVersions(true);
            var versions = new List<Tuple<Language, List<Field>, List<Tuple<int, List<Field>>>>>();
            var sharedFields = item.Fields.Where(f => !f.Name.StartsWith("__") && f.Shared && !string.IsNullOrEmpty(f.Value)).ToArray();

            foreach (var language in versionedItems.Select(i => i.Language).Distinct().ToList())
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

            output.WriteStartObject();

            output.WritePropertyString("Id", item.ID.ToString());
            output.WritePropertyString("Name", item.Name);

            if (isRoot)
            {
                output.WritePropertyString("ItemPath", item.Paths.Path);
                output.WritePropertyString("Database", item.Database.Name);
            }

            if (versions.Any() || sharedFields.Any())
            {
                output.WriteStartObject("Fields");

                foreach (var field in sharedFields.OrderBy(f => f.Name))
                {
                    output.WritePropertyStringIf(field.Name, field.Value);
                }

                if (versions.Any())
                {
                    foreach (var language in versions.OrderBy(l => l.Item1.Name))
                    {
                        output.WriteStartObject(language.Item1.Name);

                        foreach (var field in language.Item2.OrderBy(f => f.Name))
                        {
                            output.WritePropertyStringIf(field.Name, field.Value);
                        }

                        foreach (var versionedItem in language.Item3.OrderBy(t => t.Item1))
                        {
                            output.WriteStartObject(versionedItem.Item1.ToString());

                            foreach (var field in versionedItem.Item2.OrderBy(f => f.Name))
                            {
                                output.WritePropertyStringIf(field.Name, field.Value);
                            }

                            output.WriteEndObject();
                        }

                        output.WriteEndObject();
                    }

                }

                output.WriteEndObject();
            }

            if (item.Children.Any())
            {
                output.WriteStartObject("Items");
                Export(output, item.Children, false);
                output.WriteEndObject();
            }

            output.WriteEndObject();
        }
    }
}