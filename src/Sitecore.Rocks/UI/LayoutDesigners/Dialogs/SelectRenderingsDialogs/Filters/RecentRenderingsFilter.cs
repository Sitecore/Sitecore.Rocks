// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Filters
{
    [Export(typeof(IRenderingSelectorFilter))]
    public class RecentRenderingsFilter : RenderingSelectorFilterBase
    {
        public const string ControlsRenderingSelectorRecent = "Controls\\RenderingSelector\\RecentRenderings";

        public RecentRenderingsFilter()
        {
            Name = "Recent";
            Priority = 100;
        }

        [CanBeNull]
        protected IEnumerable<ItemHeader> Renderings { get; set; }

        public override void AddToRecent(ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            var s = itemHeader.Name + @"^" + itemHeader.ItemUri.ItemId + @"^" + itemHeader.TemplateName + @"^" + itemHeader.Path;

            var entries = new List<string>();

            var list = AppHost.Settings.Get(ControlsRenderingSelectorRecent, GetStorageKey(itemHeader.ItemUri.DatabaseUri), string.Empty) as string ?? string.Empty;
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

            AppHost.Settings.Set(ControlsRenderingSelectorRecent, GetStorageKey(itemHeader.ItemUri.DatabaseUri), result);
        }

        public override void GetRenderings(RenderingSelectorFilterParameters parameters, GetItemsCompleted<ItemHeader> completed)
        {
            Assert.ArgumentNotNull(parameters, nameof(parameters));
            Assert.ArgumentNotNull(completed, nameof(completed));

            var renderings = Renderings;
            if (renderings == null)
            {
                completed(Enumerable.Empty<ItemHeader>());
                return;
            }

            var list = AppHost.Settings.Get(ControlsRenderingSelectorRecent, GetStorageKey(parameters.DatabaseUri), string.Empty) as string;
            if (string.IsNullOrEmpty(list))
            {
                completed(Enumerable.Empty<ItemHeader>());
                return;
            }

            var result = new List<ItemHeader>();

            var items = list.Split('|');
            foreach (var item in items)
            {
                var parts = item.Split('^');
                if (parts.Length != 4)
                {
                    continue;
                }

                var name = parts[0];
                var itemId = new ItemId(new Guid(parts[1]));
                var templateName = parts[2];
                var path = parts[3];

                var itemUri = new ItemUri(parameters.DatabaseUri, itemId);
                if (renderings.All(t => t.ItemUri != itemUri))
                {
                    continue;
                }

                var itemHeader = new ItemHeader
                {
                    Name = name,
                    ItemUri = itemUri,
                    ParentName = Resources.Recent,
                    Path = path,
                    TemplateName = templateName
                };

                result.Add(itemHeader);
            }

            completed(result);
        }

        public override void SetRenderings(IEnumerable<ItemHeader> renderings)
        {
            Assert.ArgumentNotNull(renderings, nameof(renderings));

            Renderings = renderings;
        }

        [NotNull]
        private string GetStorageKey([NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            return databaseUri.Site.Name + @"_" + databaseUri.DatabaseName.Name;
        }
    }
}
