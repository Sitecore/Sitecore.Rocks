// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Cache;
using System.Reflection;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data
{
    public enum IconSize
    {
        Size16x16,

        Size24x24,

        Size32x32,

        Size48x48,
    }

    public class Icon : IIcon
    {
        private static readonly Dictionary<string, BitmapImage> Cache = new Dictionary<string, BitmapImage>();

        [NotNull]
        private readonly string _assemblyName;

        private readonly string _iconPath;

        private readonly bool _isResource;

        private readonly Site _site;

        private BitmapImage _emptyBitmap;

        public Icon([NotNull, Localizable(false)] string iconPath)
        {
            Assert.ArgumentNotNull(iconPath, nameof(iconPath));

            var n = iconPath.IndexOf(@";component/", StringComparison.InvariantCultureIgnoreCase);
            if (n >= 0)
            {
                _assemblyName = iconPath.Left(n);
                _iconPath = iconPath.Mid(n + 10);

                if (_assemblyName.StartsWith(@"/"))
                {
                    _assemblyName = _assemblyName.Mid(1);
                }
            }
            else
            {
                _assemblyName = @"Sitecore.Rocks";
                _iconPath = iconPath;
            }

            _isResource = true;
        }

        public Icon([NotNull, Localizable(false)] string assemblyName, [NotNull, Localizable(false)] string iconPath)
        {
            Assert.ArgumentNotNull(assemblyName, nameof(assemblyName));
            Assert.ArgumentNotNull(iconPath, nameof(iconPath));

            _assemblyName = assemblyName;
            _iconPath = iconPath;

            _isResource = true;
        }

        public Icon([NotNull] Site site, [NotNull, Localizable(false)] string iconUrl)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(iconUrl, nameof(iconUrl));

            if (site.SitecoreVersion >= Constants.Versions.Version80)
            {
                if (iconUrl.StartsWith("/~/media/", StringComparison.InvariantCultureIgnoreCase))
                {
                    iconUrl = "/sitecore/shell" + iconUrl;
                }
                else if (iconUrl.StartsWith("~/media/", StringComparison.InvariantCultureIgnoreCase))
                {
                    iconUrl = "/sitecore/shell/" + iconUrl;
                }
            }

            _site = site;
            _iconPath = iconUrl;
            _assemblyName = string.Empty;

            _isResource = false;
        }

        public BitmapSource BitmapSource => GetSource();

        [NotNull]
        public static Icon Empty { get; } = new Icon("Resources/16x16/cube_blue.png");

        [NotNull]
        public string IconPath => _iconPath ?? string.Empty;

        [NotNull]
        public BitmapImage GetSource()
        {
            var path = IconPath;

            if (string.IsNullOrEmpty(path))
            {
                return GetEmptyBitmap();
            }

            if (_isResource)
            {
                return GetResourceBitmap(path);
            }

            return GetUriBitmap(path);
        }

        [NotNull]
        public static string MakePath([Localizable(false), NotNull] string iconPath)
        {
            Assert.ArgumentNotNull(iconPath, nameof(iconPath));

            if (iconPath.StartsWith(@"/sitecore/shell", StringComparison.InvariantCultureIgnoreCase))
            {
                return iconPath;
            }

            iconPath = @"/sitecore/shell/~/icon/" + iconPath;

            if (!iconPath.EndsWith(@".aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                iconPath += ".aspx";
            }

            return iconPath;
        }

        [NotNull]
        public Icon Resize(IconSize iconSize)
        {
            if (_isResource)
            {
                return this;
            }

            var width = 16;
            switch (iconSize)
            {
                case IconSize.Size24x24:
                    width = 24;
                    break;
                case IconSize.Size32x32:
                    width = 32;
                    break;
                case IconSize.Size48x48:
                    width = 48;
                    break;
            }

            var size = @"/" + width + @"x" + width + @"/";

            var url = IconPath.Replace(@"/16x16/", size).Replace(@"/24x24/", size).Replace(@"/32x32/", size).Replace(@"/48x48/", size);

            if (url.StartsWith(@"/temp/IconCache/"))
            {
                url = @"/sitecore/shell/~/icon/" + url.Mid(16) + @".aspx";
            }

            return new Icon(_site, url);
        }

        [NotNull]
        private BitmapImage GetEmptyBitmap()
        {
            if (_emptyBitmap != null)
            {
                return _emptyBitmap;
            }

            try
            {
                var result = new BitmapImage();

                result.BeginInit();
                result.UriSource = new Uri(@"pack://application:,,,/" + _assemblyName + ";component/Resources/16x16/cube_blue.png");
                result.EndInit();

                _emptyBitmap = result;

                return result;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return new BitmapImage();
            }
        }

        [NotNull]
        private BitmapImage GetResourceBitmap([NotNull] string path)
        {
            Debug.ArgumentNotNull(path, nameof(path));

            /*
            if (AppHost.Shell.IsThemable)
            {
                var themeName = @"light";
                if (AppHost.Shell.Theme == AppTheme.Dark)
                {
                    themeName = @"dark";
                }

                var n = path.LastIndexOf(@"/", StringComparison.Ordinal);
                if (n >= 0)
                {
                    var themedPath = path.Left(n) + @"/" + themeName + path.Mid(n);

                    var result = LoadResourceBitmap(themedPath);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            */

            return LoadResourceBitmap(path) ?? GetEmptyBitmap();
        }

        [NotNull]
        private BitmapImage GetUriBitmap([NotNull] string path)
        {
            Debug.ArgumentNotNull(path, nameof(path));

            var cacheKey = path.ToLowerInvariant();

            BitmapImage image;
            if (Cache.TryGetValue(cacheKey, out image))
            {
                return image;
            }

            if (!path.StartsWith(@"http://"))
            {
                var server = _site.GetHost();
                if (!path.StartsWith(@"/"))
                {
                    path = @"/" + path;
                }

                path = server + path;
            }

            if (path.IndexOf(@"~/icon", StringComparison.Ordinal) >= 0)
            {
                path += @".aspx";
            }

            var policy = new RequestCachePolicy(RequestCacheLevel.Default);

            try
            {
                var bitmapImage = new BitmapImage(new Uri(path), policy);

                Cache[cacheKey] = bitmapImage;

                return bitmapImage;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return GetEmptyBitmap();
            }
        }

        [CanBeNull]
        private BitmapImage LoadResourceBitmap([NotNull] string path)
        {
            Debug.ArgumentNotNull(path, nameof(path));

            try
            {
                var assembly = Assembly.Load(_assemblyName);
                if (assembly == null)
                {
                    return null;
                }

                if (!assembly.ResourceExists(path))
                {
                    return null;
                }

                var result = new BitmapImage();

                result.BeginInit();
                result.UriSource = new Uri(@"pack://application:,,,/" + _assemblyName + ";component/" + path);
                result.EndInit();

                return result;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            return null;
        }
    }
}
