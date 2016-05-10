// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Server.Validations
{
    public interface IValidation
    {
        bool CanCheck([NotNull] string contextName);

        void Check([NotNull] ValidationWriter output);
    }
}
