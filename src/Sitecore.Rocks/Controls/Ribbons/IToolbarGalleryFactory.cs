// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Fluent;

namespace Sitecore.Rocks.UI.Toolbars
{
    public interface IToolbarGalleryFactory
    {
        void Create(InRibbonGallery toolbarGallery, object parameter);
    }
}
