// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell
{
    public interface IServicePane
    {
        [CanBeNull]
        object GetVsService([NotNull] Type type);
    }
}
