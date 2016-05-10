// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Packages
{
    public class DeserializeItem
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string itemPath, string contents)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var fileName = PathUtils.GetFilePath(itemPath);

            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            File.WriteAllText(fileName, contents, Encoding.UTF8);

            var options = new LoadOptions
            {
                ForceUpdate = true
            };

            var item = Manager.LoadItem(fileName, options);
            if (item == null)
            {
                return string.Empty;
            }

            /* File.Delete(fileName); */
            /* Manager.DumpItem(item); */

            return item.ID + "," + item.Database.Name;
        }
    }
}
