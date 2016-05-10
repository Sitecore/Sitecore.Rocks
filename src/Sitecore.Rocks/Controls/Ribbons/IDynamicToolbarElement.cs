// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Toolbars
{
    public interface IDynamicToolbarElement : IToolbarElement
    {
        bool CanRender([CanBeNull] object parameter);
    }
}
