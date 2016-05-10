// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.ImageExtensions;

namespace Sitecore.Rocks.Controls
{
    public class LocalMenu : Button
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var image = new Image
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0)
            };

            image.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.NearestNeighbor);
            image.SetImage("Resources/16x16/contextmenu.png");

            Click += HandleClick;
            Content = image;
        }

        [CanBeNull]
        private IContextProvider GetContextProvider()
        {
            FrameworkElement element = this;

            while (element != null)
            {
                var provider = element as IContextProvider;
                if (provider != null)
                {
                    return provider;
                }

                element = VisualTreeHelper.GetParent(element) as FrameworkElement;
            }

            return null;
        }

        private void HandleClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var contextProvider = GetContextProvider();
            if (contextProvider == null)
            {
                return;
            }

            var context = contextProvider.GetContext();
            if (context == null)
            {
                return;
            }

            var commands = CommandManager.GetCommands(context);
            if (!commands.Any())
            {
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;

            ContextMenu.IsOpen = true;

            e.Handled = true;
        }
    }
}
