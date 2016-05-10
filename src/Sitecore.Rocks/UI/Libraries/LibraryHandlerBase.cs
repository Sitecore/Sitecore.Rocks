// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;

namespace Sitecore.Rocks.UI.Libraries
{
    public abstract class LibraryHandlerBase : ILibraryHandler
    {
        public abstract bool CanHandle(string fileName, XElement element);

        public abstract LibraryBase Handle(string fileName, XElement element);
    }
}
