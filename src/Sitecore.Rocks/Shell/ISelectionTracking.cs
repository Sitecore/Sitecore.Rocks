// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell
{
    public interface ISelectionTracking
    {
        [CanBeNull]
        IEnumerable<object> GetSelectedObjects();
    }
}
