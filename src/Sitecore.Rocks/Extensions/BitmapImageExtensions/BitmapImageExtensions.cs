// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Extensions.BitmapImageExtensions
{
    public static class BitmapImageExtensions
    {
        public static void LoadIgnoreCache([NotNull] this BitmapImage bitmapImage, [NotNull] Uri uri)
        {
            bitmapImage.BeginInit();
            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmapImage.UriSource = uri;
            bitmapImage.EndInit();
        }
    }
}
