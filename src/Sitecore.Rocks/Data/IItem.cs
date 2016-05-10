// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface IItem : IItemUri
    {
        [NotNull]
        Icon Icon { get; }

        [NotNull]
        string Name { get; }
    }
}
