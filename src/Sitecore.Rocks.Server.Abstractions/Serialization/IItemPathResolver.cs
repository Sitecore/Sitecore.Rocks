using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Rocks.Server.Abstractions.Serialization
{
    public interface IItemPathResolver
    {
        string GetPath(string itemUri);
    }
}
