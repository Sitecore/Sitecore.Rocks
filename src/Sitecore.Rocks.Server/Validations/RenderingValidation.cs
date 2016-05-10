// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Validations
{
    public abstract class RenderingValidation : IRenderingValidation
    {
        public abstract bool CanCheck(string contextName, Item item, XElement renderingElement, Item renderingItem);

        public abstract void Check(ValidationWriter output, Item item, XElement renderingElement, Item renderingItem);
    }
}
