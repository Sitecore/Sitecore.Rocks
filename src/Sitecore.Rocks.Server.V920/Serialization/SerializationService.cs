using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Abstractions;
using Sitecore.Abstractions.Serialization;
using Sitecore.Data;
using Sitecore.Data.Serialization;
using Sitecore.DependencyInjection;
using ISerializationService = Sitecore.Rocks.Server.Abstractions.Serialization.ISerializationService;

namespace Sitecore.Rocks.Server.V920.Serialization
{
    public class SerializationService : ISerializationService
    {
        private readonly LazyResetable<ItemResolver> _itemResolver = ServiceLocator.GetRequiredResetableService<ItemResolver>();
        private readonly LazyResetable<PathResolver> _pathResolver = ServiceLocator.GetRequiredResetableService<PathResolver>();
        private readonly LazyResetable<BaseItemSerializationManager> _serializationManager = ServiceLocator.GetRequiredResetableService<BaseItemSerializationManager>();

        public string GetPath(string itemUri)
        {
            var item = Database.GetItem(new ItemUri(itemUri));
            var path = this._itemResolver.Value.GetItemPath(item);
            return _pathResolver.Value.GetFilePath(item.Database.Name + path);
        }

        public void SerializeTree(string itemUri)
        {
            var item = Database.GetItem(new ItemUri(itemUri));
            _serializationManager.Value.DumpTree(item);
        }

        public void SerializeItem(string itemUri)
        {
            var item = Database.GetItem(new ItemUri(itemUri));
            _serializationManager.Value.DumpItem(item);
        }

        public void UpdateTree(string itemUri, bool forceUpdate)
        {
            var options = new LoadOptions
            {
                ForceUpdate = forceUpdate,
            };
            var item = Database.GetItem(new ItemUri(itemUri));
            _serializationManager.Value.LoadTree(item, options);
        }

        public void UpdateItem(string itemUri, bool forceUpdate)
        {
            var options = new LoadOptions
            {
                ForceUpdate = forceUpdate,
            };
            var item = Database.GetItem(new ItemUri(itemUri));
            _serializationManager.Value.LoadItem(item, options);
        }
    }
}
