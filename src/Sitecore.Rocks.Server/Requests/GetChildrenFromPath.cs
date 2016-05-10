// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetChildrenFromPath
    {
        [NotNull]
        public string Execute([NotNull] string path, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return string.Empty;
            }

            var item = database.GetItem(path);
            if (item == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var output = new StringWriter(builder);

            foreach (Item child in item.Children)
            {
                output.WriteLine(child.Paths.Path);
            }

            return builder.ToString();
        }
    }
}
