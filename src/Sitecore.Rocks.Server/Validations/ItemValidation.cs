// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Validations
{
    public abstract class ItemValidation : IItemValidation
    {
        public abstract bool CanCheck(string contextName, Item item);

        public abstract void Check(ValidationWriter output, Item item);
    }
}
