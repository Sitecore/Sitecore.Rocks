// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.Commandy.Modes;

namespace Sitecore.Rocks.UI.Commandy
{
    public interface IDefaultCommandyMode
    {
        [CanBeNull]
        IMode GetCommandyMode([NotNull] Commandy commandy);
    }
}
