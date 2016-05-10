// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Globalization;
using Sitecore.Rocks.Server.Pipelines.GetFieldValue;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;

namespace Sitecore.Rocks.Server.Requests.Layouts
{
    public class SaveLayout
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId, [NotNull] string languageName, [NotNull] string version, [NotNull] string fieldName, [NotNull] string xml)
        {
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(itemId, Language.Parse(languageName), Data.Version.Parse(version));
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var field = item.Fields[fieldName];

            item.Editing.BeginEdit();
            SetFieldValuePipeline.Run().WithParameters(field, xml);
            item.Editing.EndEdit();

            item = database.GetItem(itemId);

            var pipeline = GetFieldValuePipeline.Run().WithParameters(item, fieldName);

            return pipeline.Value;
        }
    }
}
