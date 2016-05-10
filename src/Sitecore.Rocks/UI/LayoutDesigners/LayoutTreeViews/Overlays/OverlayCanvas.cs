// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Overlays
{
    public class OverlayCanvas : Canvas
    {
        public void Add([NotNull] UIElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Children.Add(element);
            Background = Brushes.Transparent;

            element.Focus();
            Keyboard.Focus(element);
        }

        public void CloseAll()
        {
            var windows = Children.OfType<OverlayWindow>().ToList();

            for (var index = windows.Count - 1; index >= 0; index--)
            {
                var window = windows[index];
                window.IsOpen = false;
            }
        }

        public void EndDragging()
        {
            Background = Children.Count > 0 ? Brushes.Transparent : null;
        }

        public void Remove([NotNull] OverlayWindow element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            Children.Remove(element);

            if (Children.Count == 0)
            {
                Background = null;
            }
        }

        public void StartDragging()
        {
            Background = null;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnMouseDown(e);

            CloseAll();
        }
    }
}
