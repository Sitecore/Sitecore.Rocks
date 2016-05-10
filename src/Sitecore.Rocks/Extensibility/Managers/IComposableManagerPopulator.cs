// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Extensibility.Managers
{
    public interface IComposableManagerPopulator<T> where T : class
    {
        [NotNull]
        IEnumerable<T> Populate();
    }
}
