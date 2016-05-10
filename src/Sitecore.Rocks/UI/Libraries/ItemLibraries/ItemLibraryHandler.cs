// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml.Linq;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Libraries.ItemLibraries
{
    [LibraryHandler]
    public class ItemLibraryHandler : LibraryHandlerBase
    {
        public override bool CanHandle(string fileName, XElement element)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(element, nameof(element));

            return element.Name == "items";
        }

        public override LibraryBase Handle(string fileName, XElement element)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(element, nameof(element));

            var result = new ItemLibrary(fileName, Path.GetFileNameWithoutExtension(fileName) ?? "Unknown");

            result.Initialize();

            return result;
        }
    }
}
