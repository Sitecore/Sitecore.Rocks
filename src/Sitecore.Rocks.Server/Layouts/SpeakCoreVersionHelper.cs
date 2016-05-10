// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Layouts
{
    public class SpeakCoreVersionHelper
    {
        public static readonly ID SpeakRenderingFolderId = new ID(new Guid("{BAAD9D10-19E7-4878-A96F-E290B914BF5F}"));

        [NotNull]
        public string GetSpeakCoreVersionId([NotNull] Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var speakCoreVersionItem = GetSpeakCoreVersionRenderingFolderItem(item);
            return speakCoreVersionItem != null ? speakCoreVersionItem["SpeakCoreVersion"] : string.Empty;
        }

        [CanBeNull]
        public Item GetSpeakCoreVersionItem([NotNull] Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var itemId = GetSpeakCoreVersionId(item);
            return string.IsNullOrEmpty(itemId) ? null : item.Database.GetItem(itemId);
        }

        [CanBeNull]
        public Item GetSpeakCoreVersionRenderingFolderItem([NotNull] Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            while (item != null)
            {
                if (item.TemplateID == SpeakRenderingFolderId)
                {
                    var version = item["SpeakCoreVersion"];
                    if (!string.IsNullOrEmpty(version))
                    {
                        return item;
                    }
                }

                item = item.Parent;
            }

            return null;
        }
    }
}
