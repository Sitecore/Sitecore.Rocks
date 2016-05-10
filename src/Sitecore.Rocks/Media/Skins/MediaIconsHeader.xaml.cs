// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Net.Cache;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Media.Skins
{
    public partial class MediaIconsHeader
    {
        public MediaIconsHeader()
        {
            InitializeComponent();
        }

        public void Initialize([NotNull] ItemHeader itemHeader, int width, int height)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            ItemName.Text = itemHeader.Name;
            IconImage.Width = width;
            IconImage.Height = height;
            Grid.MaxWidth = width;

            var path = string.Format(@"{0}{1}.ashx?bc=White&db={2}&h={3}la=en&thn=1&w={4}", MediaManager.GetMediaUrl(itemHeader.ItemUri.Site), itemHeader.ItemId.ToShortId(), itemHeader.ItemUri.DatabaseName.Name, height, width);

            var policy = new RequestCachePolicy(RequestCacheLevel.Default);

            try
            {
                IconImage.Source = new BitmapImage(new Uri(path), policy);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
                IconImage.Source = null;
            }
        }
    }
}
