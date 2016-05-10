// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Diagnostics
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public abstract class FriendlyException : Exception
    {
        protected FriendlyException()
        {
        }

        protected FriendlyException([CanBeNull] string message) : base(message)
        {
        }

        protected FriendlyException([NotNull] string message, [NotNull] Exception innerException) : base(message, innerException)
        {
            Assert.ArgumentNotNull(message, nameof(message));
            Assert.ArgumentNotNull(innerException, nameof(innerException));
        }

        public abstract bool Handle();
    }
}
