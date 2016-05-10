// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Net.Cache;
using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Media.Skins.Tiles
{
    public partial class TileHeader
    {
        public TileHeader()
        {
            InitializeComponent();
        }

        [NotNull]
        public void Initialize([NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            ItemName.Text = itemHeader.Name;
            TemplateName.Text = itemHeader.TemplateName;
            Updated.Text = itemHeader.Updated.ToString();

            var path = MediaManager.GetMediaUrl(itemHeader.ItemUri.Site) + itemHeader.ItemId.ToShortId() + @".ashx?bc=White&db=" + itemHeader.ItemUri.DatabaseName.Name + @"&h=48&la=en&thn=1&w=48";

            var policy = new RequestCachePolicy(RequestCacheLevel.Default);

            IconImage.Source = new BitmapImage(new Uri(path), policy);
        }
    }
}
