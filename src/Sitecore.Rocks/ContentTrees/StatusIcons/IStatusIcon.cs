// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.StatusIcons
{
    public interface IStatusIcon
    {
        [CanBeNull]
        Image GetStatus([NotNull] ItemHeader item);
    }
}
