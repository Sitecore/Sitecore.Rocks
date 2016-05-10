// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml.Linq;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Libraries.SearchLibraries
{
    [LibraryHandler]
    public class SearchLibraryHandler : LibraryHandlerBase
    {
        public override bool CanHandle(string fileName, XElement element)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(element, nameof(element));

            return element.Name == "search";
        }

        public override LibraryBase Handle(string fileName, XElement element)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(element, nameof(element));

            var result = new SearchLibrary(fileName, Path.GetFileNameWithoutExtension(fileName) ?? "Unknown");

            result.Initialize();

            return result;
        }
    }
}
