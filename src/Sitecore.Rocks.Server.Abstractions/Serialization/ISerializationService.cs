using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Rocks.Server.Abstractions.Serialization
{
    public interface ISerializationService
    {
        string GetPath(string itemUri);
        void SerializeTree(string itemUri);
        void SerializeItem(string itemUri);
        void UpdateTree(string itemUri, bool forceUpdate);
        void UpdateItem(string itemUri, bool forceUpdate);
    }
}
