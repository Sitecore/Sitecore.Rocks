// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Caches.Dialogs.CacheExplorer
{
    public partial class CacheExplorerDialog : IContextProvider
    {
        private readonly List<CacheKeyDescriptor> caches = new List<CacheKeyDescriptor>();

        private readonly ListViewSorter listViewSorter;

        public CacheExplorerDialog([NotNull] Site site, [NotNull] string cacheName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(cacheName, nameof(cacheName));

            InitializeComponent();
            this.InitializeDialog();

            Site = site;
            CacheName = cacheName;
            FilterText = string.Empty;

            CacheNameLabel.Content = "Cache Keys in " + CacheName + ":";
            listViewSorter = new ListViewSorter(CacheKeyList);

            LoadCacheKeys();
        }

        [NotNull]
        public string CacheName { get; }

        [NotNull]
        public Site Site { get; set; }

        [NotNull]
        protected string FilterText { get; set; }

        [NotNull]
        public object GetContext()
        {
            return new CacheExplorerContext(this);
        }

        public void LoadCacheKeys()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.HideLoading(ContextMenuPanel);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseCacheKeys(root);
                RenderCacheKeys();

                Keyboard.Focus(CacheKeyList);
            };

            Loading.ShowLoading(ContextMenuPanel);

            Site.DataService.ExecuteAsync("Caches.GetCacheKeys", callback, CacheName);
        }

        public void Refresh()
        {
            LoadCacheKeys();
        }

        private void CloseClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderCacheKeys();
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenuPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseCacheKeys([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            var list = new List<CacheKeyDescriptor>();

            foreach (var element in root.Elements())
            {
                var cacheKeyDescriptor = new CacheKeyDescriptor
                {
                    Key = element.GetAttributeValue("key"),
                    Size = element.GetAttributeLong("size", 0),
                    Data = element.Value,
                    LastAccessed = DateTimeExtensions.FromIso(element.GetAttributeValue("lastAccessed"))
                };

                list.Add(cacheKeyDescriptor);
            }

            caches.Clear();
            caches.AddRange(list);
        }

        private void RenderCacheKeys()
        {
            var list = new List<CacheKeyDescriptor>();
            foreach (var cacheDescriptor in caches)
            {
                if (cacheDescriptor.Key.IsFilterMatch(FilterText))
                {
                    list.Add(cacheDescriptor);
                }
            }

            CacheKeyList.ItemsSource = null;
            CacheKeyList.ItemsSource = list.OrderBy(c => c.Key);
            listViewSorter.Resort();

            CacheKeyList.ResizeColumn(KeyColumn);
            CacheKeyList.ResizeColumn(SizeColumn);
            CacheKeyList.ResizeColumn(LastAccessedColumn);
            CacheKeyList.ResizeColumn(DataColumn);

            if (caches.Count > 0)
            {
                CacheKeyList.SelectedIndex = 0;
            }
        }

        public class CacheKeyDescriptor
        {
            public string Data { get; set; }

            [NotNull]
            public string FormattedLastAccessed
            {
                get { return LastAccessed.ToString(); }
            }

            [NotNull]
            public string FormattedSize
            {
                get { return Size.ToString("#,##0 bytes"); }
            }

            public string Key { get; set; }

            public DateTime LastAccessed { get; set; }

            public long Size { get; set; }
        }
    }
}
