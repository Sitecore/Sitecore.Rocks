using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Serialization;
using Sitecore.Rocks.Server.Abstractions.Serialization;

namespace Sitecore.Rocks.Server.V910.Serialization
{
    public class ItemPathResolver : IItemPathResolver
    {
        public string GetPath(string itemUri)
        {
            var item = Database.GetItem(new ItemUri(itemUri));
            var reference = new ItemReference(item);
            return PathUtils.GetFilePath(reference.ToString());
        }
    }
}
