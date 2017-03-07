// © 2015-2017 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
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

            using (var writer = new StreamWriter(fileName))
            {
                Export(new YamlTextWriter(writer), item);
            }

            return fileName;
        }

        private void Export(YamlTextWriter output, Item item)
        {
            output.WriteStartElement(item.TemplateName, item.Name);
            output.WriteAttributeString("ItemPath", item.Paths.Path);
            output.WriteAttributeString("Database", item.Database.Name);
            output.WriteAttributeString("ID", item.ID.ToString());

            item.Fields.ReadAll();

            var versionedItems = item.Versions.GetVersions(true);
            var versions = new List<Tuple<Language, List<Field>, List<Tuple<int, List<Field>>>>>();

            foreach (var language in versionedItems.Select(i => i.Language).Distinct().ToList())
            {
                var unversionedItem = versionedItems.FirstOrDefault(i => i.Language == language);
                if (unversionedItem == null)
                {
                    continue;
                }

                unversionedItem.Fields.ReadAll();

                var unversionedFields = unversionedItem.Fields.Where(f => !f.Name.StartsWith("__") && !f.Shared && f.Unversioned).ToList();

                var versionedFields = new List<Tuple<int, List<Field>>>();
                foreach (var versionedItem in versionedItems.Where(i => i.Language == language))
                {
                    versionedItem.Fields.ReadAll();

                    var flds = versionedItem.Fields.Where(f => !f.Name.StartsWith("__") && !f.Shared && !f.Unversioned).ToList();
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

            foreach (var field in item.Fields.Where(f => !f.Name.StartsWith("__") && f.Shared).OrderBy(f => f.Name))
            {
                output.WriteAttributeString(field.Name, field.Value);
            }

            if (versions.Any())
            {
                output.WriteStartElement("..Versions");

                foreach (var language in versions.OrderBy(l => l.Item1.Name))
                {
                    output.WriteStartElement(language.Item1.Name);

                    foreach (var field in language.Item2.OrderBy(f => f.Name))
                    {
                        output.WriteAttributeStringIf(field.Name, field.Value);
                    }

                    foreach (var versionedItem in language.Item3.OrderBy(t => t.Item1))
                    {
                        output.WriteStartElement(versionedItem.Item1.ToString());

                        foreach (var field in versionedItem.Item2.OrderBy(f => f.Name))
                        {
                            output.WriteAttributeString(field.Name, field.Value);
                        }

                        output.WriteEndElement();
                    }

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            foreach (Item child in item.Children)
            {
                Export(output, child);
            }

            output.WriteEndElement();
        }
    }
}
