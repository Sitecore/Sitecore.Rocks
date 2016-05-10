// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class SetFieldValue
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id, [NotNull] string language, [NotNull] string version, [NotNull] string fieldName, [NotNull] string value)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(language, nameof(language));
            Assert.ArgumentNotNull(version, nameof(version));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(value, nameof(value));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var l = LanguageManager.GetLanguage(language);
            var v = Data.Version.Parse(version);

            var item = database.GetItem(id, l, v);
            if (database == null)
            {
                throw new Exception("Item not found");
            }

            item.Editing.BeginEdit();
            SetFieldValuePipeline.Run().WithParameters(item, fieldName, value);
            item.Editing.EndEdit();

            item.Fields.ReadAll();
            var field = item.Fields[fieldName];
            return field == null ? string.Empty : field.ID + "," + item.Version;
        }
    }
}
