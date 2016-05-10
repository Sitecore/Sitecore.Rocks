// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Rocks.Server.Layouts;

namespace Sitecore.Rocks.Server.Requests.Layouts
{
    public class GetLayoutPlaceHolders
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemId)
        {
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new InvalidOperationException("Database not found");
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                throw new InvalidOperationException("Item not found");
            }

            var placeHolders = PlaceHolderAnalyzer.Analyze(item);

            return string.Join(",", placeHolders.ToArray());
        }
    }
}
