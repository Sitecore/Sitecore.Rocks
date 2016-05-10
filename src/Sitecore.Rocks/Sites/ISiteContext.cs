// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Sites
{
    public interface ISiteContext
    {
        [NotNull]
        Site Site { get; }

        void SetSite([NotNull] Site site);
    }
}
