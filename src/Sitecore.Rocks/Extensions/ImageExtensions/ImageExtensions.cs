// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.IO;
using System.Net.Cache;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Extensions.ImageExtensions
{
    public static class ImageExtensions
    {
        public static void SetImage([NotNull] this Image image, [NotNull, Localizable(false)] string imagePath)
        {
            Assert.ArgumentNotNull(image, nameof(image));
            Assert.ArgumentNotNull(imagePath, nameof(imagePath));

            if (!imagePath.StartsWith(@"/") && !imagePath.StartsWith(@"http"))
            {
                BitmapImage result;
                try
                {
                    result = new BitmapImage();

                    result.BeginInit();
                    result.UriSource = new Uri("pack://application:,,,/Sitecore.Rocks;component/" + imagePath);
                    result.EndInit();
                }
                catch (IOException ex)
                {
                    AppHost.Output.LogException(ex);
                    result = Icon.Empty.GetSource();
                }

                image.Source = result;
            }
            else
            {
                var policy = new RequestCachePolicy(RequestCacheLevel.Default);
                image.Source = new BitmapImage(new Uri(imagePath), policy);
            }
        }

        public static void SetImage([NotNull] this Image image, [NotNull] Site site, [NotNull] string imagePath)
        {
            Assert.ArgumentNotNull(image, nameof(image));
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(imagePath, nameof(imagePath));

            if (!imagePath.StartsWith(@"/") && !imagePath.StartsWith(@"http"))
            {
                SetImage(image, imagePath);
            }
            else
            {
                var server = site.GetHost();

                var url = server + imagePath;

                if (url.IndexOf(@"~/icon", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    url += @".aspx";
                }

                var policy = new RequestCachePolicy(RequestCacheLevel.Default);
                try
                {
                    image.Source = new BitmapImage(new Uri(url), policy);
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                    image.Source = Icon.Empty.GetSource();
                }
            }
        }
    }
}
