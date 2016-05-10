// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.ContentEditors.InfoPanes
{
    public interface IInfoPane
    {
        bool CanRender([NotNull] ContentEditor contentEditor);

        [NotNull]
        object GetHeader();

        [NotNull]
        FrameworkElement Render([NotNull] ContentEditor contentEditor);
    }
}
