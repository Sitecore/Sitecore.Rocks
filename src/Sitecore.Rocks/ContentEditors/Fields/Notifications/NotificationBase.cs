// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Fields.Notifications
{
    public abstract class NotificationBase : Border
    {
        public void Hide()
        {
            Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        protected void Initialize([NotNull] string text, bool hidden)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            InitializeStyle();
            InitializeText(text);

            if (hidden)
            {
                Visibility = Visibility.Hidden;
            }
        }

        protected void InitializeStyle()
        {
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            Background = SystemColors.InfoBrush;
            BorderBrush = SystemColors.ActiveBorderBrush;
            BorderThickness = new Thickness(1.0);
            CornerRadius = new CornerRadius(3.0);
            Padding = new Thickness(5.0);
            Margin = new Thickness(20.0);
            Opacity = 0.9;
            SnapsToDevicePixels = true;
        }

        protected void InitializeText([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            Child = new TextBlock
            {
                Text = text
            };
        }
    }
}
