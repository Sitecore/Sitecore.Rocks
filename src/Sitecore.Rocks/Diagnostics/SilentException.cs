// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Diagnostics
{
    [UsedImplicitly]
    public class SilentException : FriendlyException
    {
        public SilentException() : base(string.Empty)
        {
        }

        public override bool Handle()
        {
            return true;
        }
    }
}
