// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.Validations
{
    public interface IItemValidation
    {
        bool CanCheck([NotNull] string contextName, [NotNull] Item item);

        void Check([NotNull] ValidationWriter output, [NotNull] Item item);
    }
}
