// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.ContentTrees.StatusIcons
{
    [Export(typeof(IStatusIcon))]
    public class SerializationStatusIcon : IStatusIcon
    {
        [NotNull]
        private static readonly BitmapImage Modified;

        [NotNull]
        private static readonly BitmapImage Serialized;

        static SerializationStatusIcon()
        {
            Modified = new Icon("Resources/7x16/StatusModified.png").GetSource();
            Serialized = new Icon("Resources/7x16/StatusNormal.png").GetSource();
        }

        public Image GetStatus(ItemHeader item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            BitmapImage icon;
            string tooltip;

            switch (item.SerializationStatus)
            {
                case SerializationStatus.Serialized:
                    icon = Serialized;
                    tooltip = "Serialized.";
                    break;
                case SerializationStatus.Modified:
                    icon = Modified;
                    tooltip = "Modified - must be serialized.";
                    break;
                default:
                    return null;
            }

            var result = new Image()
            {
                Source = icon,
                ToolTip = tooltip,
                SnapsToDevicePixels = true,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 7,
                Height = 16
            };

            return result;
        }
    }
}
