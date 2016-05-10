// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Validations
{
    public interface IRenderingValidation
    {
        bool CanCheck([NotNull] string contextName, [NotNull] Item item, [NotNull] XElement renderingElement, [NotNull] Item renderingItem);

        void Check([NotNull] ValidationWriter output, [NotNull] Item item, [NotNull] XElement renderingElement, [NotNull] Item renderingItem);
    }
}
