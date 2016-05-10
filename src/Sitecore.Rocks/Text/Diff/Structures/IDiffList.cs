// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Text.Diff.Structures
{
    public interface IDiffList
    {
        int Count();

        [NotNull]
        IComparable GetByIndex(int index);
    }
}
