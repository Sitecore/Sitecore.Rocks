// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.StartPage
{
    public interface IStartPageControl
    {
        [NotNull]
        string ParentName { get; }

        [NotNull]
        string Text { get; set; }

        [CanBeNull]
        FrameworkElement GetControl([NotNull] string parentName);
    }
}
