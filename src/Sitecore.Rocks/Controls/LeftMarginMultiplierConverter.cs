// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public class LeftMarginMultiplierConverter : IValueConverter
    {
        public double Length { get; set; }

        [NotNull]
        public object Convert([CanBeNull] object value, [CanBeNull] Type targetType, [CanBeNull] object parameter, [CanBeNull] CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
            {
                return new Thickness(0);
            }

            return new Thickness(0, 0, Length * GetDepth(item), 0);
        }

        [CanBeNull]
        public object ConvertBack([CanBeNull] object value, [CanBeNull] Type targetType, [CanBeNull] object parameter, [CanBeNull] CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static int GetDepth([NotNull] TreeViewItem item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            FrameworkElement elem = item;

            while (elem != null && elem.Parent != null)
            {
                var tvi = elem.Parent as TreeViewItem;
                if (tvi != null)
                {
                    return GetDepth(tvi) + 1;
                }

                elem = elem.Parent as FrameworkElement;
            }

            return 0;
        }
    }
}
