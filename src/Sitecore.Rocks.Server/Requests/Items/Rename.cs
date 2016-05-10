// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Jobs;
using Sitecore.SecurityModel;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class Rename
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, [NotNull] string newName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            item.Editing.BeginEdit();
            item.Name = newName;
            item.Editing.EndEdit();

            BackgroundJob.Run("Rename Item", "Updating Items", () => Update(item));

            return string.Empty;
        }

        private void Update([NotNull] Item item)
        {
            using (new SecurityDisabler())
            {
                var itemLinks = Globals.LinkDatabase.GetReferrers(item);
                if (itemLinks.Length > 0)
                {
                    foreach (var itemLink in itemLinks)
                    {
                        var database = Factory.GetDatabase(itemLink.SourceDatabaseName);
                        if (database == null)
                        {
                            continue;
                        }

                        var source = database.GetItem(itemLink.SourceItemID);
                        if (source == null)
                        {
                            continue;
                        }

                        foreach (var version in source.Versions.GetVersions())
                        {
                            var sourceField = version.Fields[itemLink.SourceFieldID];
                            if (sourceField == null)
                            {
                                continue;
                            }

                            var customField = FieldTypeManager.GetField(sourceField);
                            if (customField == null)
                            {
                                continue;
                            }

                            version.Editing.BeginEdit();
                            customField.UpdateLink(itemLink);
                            version.Editing.EndEdit();
                        }
                    }
                }
            }

            foreach (Item child in item.Children)
            {
                Update(child);
            }
        }
    }
}
