// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class SaveTemplateFieldSorterFields
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string xml)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(xml, nameof(xml));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var doc = XDocument.Parse(xml);

            var root = doc.Root;
            if (root == null)
            {
                return string.Empty;
            }

            foreach (var element in root.Elements())
            {
                var templateFieldId = element.GetAttributeValue("id");
                var sortOrder = element.GetAttributeValue("sortorder");
                var sectionSortOrder = element.GetAttributeValue("sectionsortorder");

                var item = database.GetItem(templateFieldId);
                if (item == null)
                {
                    continue;
                }

                int value;
                int.TryParse(sortOrder, out value);

                item.Editing.BeginEdit();
                item.Appearance.Sortorder = value;
                item.Editing.EndEdit();

                var section = item.Parent;
                if (section == null)
                {
                    continue;
                }

                int.TryParse(sectionSortOrder, out value);

                section.Editing.BeginEdit();
                section.Appearance.Sortorder = value;
                section.Editing.EndEdit();
            }

            return string.Empty;
        }
    }
}
