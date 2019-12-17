using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Serialization;
using Sitecore.DependencyInjection;
using Sitecore.Rocks.Server.Abstractions.Serialization;

namespace Sitecore.Rocks.Server.V920.Serialization
{
    public class ItemPathResolver : IItemPathResolver
    {
        private readonly LazyResetable<ItemResolver> _itemResolver = ServiceLocator.GetRequiredResetableService<ItemResolver>();
        private readonly LazyResetable<PathResolver> _pathResolver = ServiceLocator.GetRequiredResetableService<PathResolver>();

        public string GetPath(string itemUri)
        {
            var item = Database.GetItem(new ItemUri(itemUri));
            var path = this._itemResolver.Value.GetItemPath(item);
            return _pathResolver.Value.GetFilePath(item.Database.Name + path);
        }
    }
}
