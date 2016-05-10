// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Publishing;

namespace Sitecore.Rocks.Server.Requests.Publishing
{
    public class PublishItemAndRelatedItems : PublishItem
    {
        protected override void Publish(List<string> links, Item item, Database[] targetDatabases, Language[] languages, bool deep, bool compareRevisions)
        {
            Debug.ArgumentNotNull(links, nameof(links));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(targetDatabases, nameof(targetDatabases));
            Debug.ArgumentNotNull(languages, nameof(languages));

            var key = item.Database.Name + item.ID;
            if (links.Contains(key))
            {
                return;
            }

            links.Add(key);

            PublishManager.PublishItem(item, targetDatabases, languages, deep, compareRevisions, true);
        }
    }
}
