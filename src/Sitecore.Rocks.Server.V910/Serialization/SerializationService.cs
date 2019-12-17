using System;
using Sitecore.Data;
using Sitecore.Data.Serialization;
using Sitecore.Rocks.Server.Abstractions.Serialization;

namespace Sitecore.Rocks.Server.V910.Serialization
{
    public class SerializationService : ISerializationService
    {
        public string GetPath(string itemUri)
        {
            var item = Database.GetItem(new ItemUri(itemUri));
            var reference = new ItemReference(item);
            return PathUtils.GetFilePath(reference.ToString());
        }

        public void SerializeTree(string itemUri)
        {
            var item = Database.GetItem(new ItemUri(itemUri));
            Manager.DumpTree(item);
        }

        public void SerializeItem(string itemUri)
        {
            var item = Database.GetItem(new ItemUri(itemUri));
            Manager.DumpItem(item);
        }

        public void UpdateTree(string itemUri, bool forceUpdate)
        {
            var options = new LoadOptions
            {
                ForceUpdate = forceUpdate,
            };
            var item = Database.GetItem(new ItemUri(itemUri));
            var directory = PathUtils.GetDirectoryPath(new ItemReference(item).ToString());
            Manager.LoadTree(directory, options);
        }

        public void UpdateItem(string itemUri, bool forceUpdate)
        {
            var options = new LoadOptions
            {
                ForceUpdate = forceUpdate,
            };
            var item = Database.GetItem(new ItemUri(itemUri));
            var fileName = PathUtils.GetFilePath(new ItemReference(item).ToString());
            Manager.LoadItem(fileName, options);
        }
    }
}
