// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers
{
    public abstract class BaseDesigner
    {
        [NotNull]
        public string Name { get; protected set; }

        public abstract bool CanDesign([CanBeNull] object parameter);

        [CanBeNull]
        public abstract FrameworkElement GetDesigner([CanBeNull] object parameter);
    }
}
