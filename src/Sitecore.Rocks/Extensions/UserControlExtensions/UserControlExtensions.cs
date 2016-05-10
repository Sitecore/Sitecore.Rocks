// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.UserControlExtensions
{
    public static class UserControlExtensions
    {
        public static void InitializeToolBar([NotNull] this UserControl userControl, [NotNull] object sender)
        {
            Assert.ArgumentNotNull(userControl, nameof(userControl));
            Assert.ArgumentNotNull(sender, nameof(sender));

            var toolBar = sender as ToolBar;
            if (toolBar == null)
            {
                return;
            }

            var border = toolBar.Template.FindName(@"border", toolBar) as Border;
            if (border != null)
            {
                border.BorderThickness = new Thickness(0);
            }

            var overflowGrid = toolBar.Template.FindName(@"OverflowGrid", toolBar) as Grid;
            if (overflowGrid == null)
            {
                overflowGrid = toolBar.Template.FindName(@"overflowGrid", toolBar) as Grid;
            }

            if (overflowGrid == null)
            {
                return;
            }

            var toggleButton = overflowGrid.Children[0] as ToggleButton;
            if (toggleButton != null)
            {
                toggleButton.Background = Brushes.Transparent;
            }
        }
    }
}
