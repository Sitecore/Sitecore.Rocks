// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data
{
    public interface ISiteSelectionContext
    {
        [NotNull]
        Site Site { get; }
    }
}
