// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Server.Validations
{
    public abstract class Validation : IValidation
    {
        public abstract bool CanCheck(string contextName);

        public abstract void Check(ValidationWriter output);
    }
}
