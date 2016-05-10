// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface ICanDeleteWithText : ICanDelete
    {
        [NotNull]
        string CommandText { get; }

        [NotNull]
        string MultipleText { get; }

        [NotNull]
        string SingleText { get; }
    }
}
