// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface IHasFileUri
    {
        [NotNull]
        FileUri FileUri { get; }
    }
}
