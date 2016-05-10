// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.TemplateSelector
{
    public static class RecentTemplateManager
    {
        public const string ControlsTemplateSelectorRecent = "Controls\\TemplateSelector\\RecentTemplates";

        public static void AddToRecent([NotNull] TemplateHeader templateHeader)
        {
            Assert.ArgumentNotNull(templateHeader, nameof(templateHeader));

            var s = templateHeader.Name + @"^" + templateHeader.TemplateUri.ItemId + @"^" + templateHeader.Path;

            var entries = new List<string>();

            var list = AppHost.Settings.Get(ControlsTemplateSelectorRecent, GetStorageKey(templateHeader.TemplateUri.DatabaseUri), string.Empty) as string ?? string.Empty;
            if (!string.IsNullOrEmpty(list))
            {
                entries.AddRange(list.Split('|'));
            }

            entries.Remove(s);
            entries.Insert(0, s);

            while (entries.Count > 10)
            {
                entries.RemoveAt(10);
            }

            var result = string.Join("|", entries);

            AppHost.Settings.Set(ControlsTemplateSelectorRecent, GetStorageKey(templateHeader.TemplateUri.DatabaseUri), result);
        }

        [NotNull]
        public static IEnumerable<TemplateHeader> GetTemplates([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var list = AppHost.Settings.Get(ControlsTemplateSelectorRecent, GetStorageKey(databaseUri), string.Empty) as string;
            if (string.IsNullOrEmpty(list))
            {
                return Enumerable.Empty<TemplateHeader>();
            }

            var result = new List<TemplateHeader>();

            var items = list.Split('|');
            foreach (var item in items)
            {
                var parts = item.Split('^');

                var name = parts[0];
                var itemId = new ItemId(new Guid(parts[1]));
                var path = parts[2];

                var templateUri = new ItemUri(databaseUri, itemId);

                var template = new TemplateHeader(templateUri, name, string.Empty, path, Resources.Recent, false);

                result.Add(template);
            }

            return result;
        }

        [NotNull]
        private static string GetStorageKey([NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            return databaseUri.Site.Name + @"_" + databaseUri.DatabaseName.Name;
        }
    }
}
