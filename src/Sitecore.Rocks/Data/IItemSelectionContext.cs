// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface IItemSelectionContext
    {
        [NotNull]
        IEnumerable<IItem> Items { get; }
    }
}
