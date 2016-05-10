// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Libraries
{
    public interface ILibraryHandler
    {
        bool CanHandle([NotNull] string fileName, [NotNull] XElement element);

        [NotNull]
        LibraryBase Handle([NotNull] string fileName, [NotNull] XElement element);
    }
}
