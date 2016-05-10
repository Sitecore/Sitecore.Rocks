// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetSubitemSortings
    {
        [NotNull]
        public string Execute([NotNull] string id, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id);
            if (item == null)
            {
                return string.Empty;
            }

            var selected = item[FieldIDs.SubitemsSorting];

            var root = database.GetItem("/sitecore/system/Settings/Subitems Sorting");
            if (root == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();

            foreach (Item child in root.Children)
            {
                result.Append(child.ID);

                result.Append(child.ID.ToString() == selected ? "1" : "0");

                result.AppendLine(child.DisplayName);
            }

            return result.ToString();
        }
    }
}
