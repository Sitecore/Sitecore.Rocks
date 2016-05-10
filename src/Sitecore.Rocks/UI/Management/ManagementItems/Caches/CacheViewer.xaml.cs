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
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.Management.ManagementItems.Caches.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Caches
{
    [Management(ItemName, 5000)]
    public partial class CacheViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Caches";

        private readonly List<CacheDescriptor> _caches = new List<CacheDescriptor>();

        private readonly ListViewSorter _listViewSorter;

        public CacheViewer()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(CacheList);
            FilterText = string.Empty;

            Loaded += ControlLoaded;
        }

        public SiteManagementContext Context { get; set; }

        protected string FilterText { get; set; }

        public bool CanExecute(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var site = context as SiteManagementContext;
            if (site == null)
            {
                return false;
            }

            return site.Site.DataService.CanExecuteAsync("Caches.GetCaches");
        }

        [NotNull]
        public object GetContext()
        {
            return new CacheViewerContext(this);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (SiteManagementContext)context;

            return this;
        }

        public void LoadCaches()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.Swap(CacheList);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseCaches(root);
                RenderCaches();
            };

            Loading.ShowLoading(CacheList);

            Context.Site.DataService.ExecuteAsync("Caches.GetCaches", callback);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadCaches();
        }

        private void ExploreCache([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var command = new CacheExplorer();
            var context = GetContext();

            if (command.CanExecute(context))
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            }
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderCaches();
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            _listViewSorter.HeaderClick(sender, e);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseCaches([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            var list = new List<CacheDescriptor>();

            foreach (var element in root.Elements())
            {
                var cacheDescriptor = new CacheDescriptor
                {
                    Name = element.GetAttributeValue("name"),
                    Size = element.GetAttributeLong("size", 0),
                    Count = element.GetAttributeLong("count", 0),
                    MaxSize = element.GetAttributeLong("maxsize", 0),
                    Scavengable = element.GetAttributeValue("scavengable") == "true",
                    Enabled = element.GetAttributeValue("enabled") == "true",
                    Priority = element.GetAttributeValue("priority"),
                    Delta = 0
                };

                var oldCache = _caches.FirstOrDefault(c => c.Name == cacheDescriptor.Name);
                if (oldCache != null)
                {
                    cacheDescriptor.Delta = cacheDescriptor.Size - oldCache.Size;
                }
                else
                {
                    cacheDescriptor.Delta = cacheDescriptor.Size;
                }

                list.Add(cacheDescriptor);
            }

            _caches.Clear();
            _caches.AddRange(list);
        }

        private void RenderCaches()
        {
            var list = new List<CacheDescriptor>();
            foreach (var cacheDescriptor in _caches)
            {
                if (cacheDescriptor.Name.IsFilterMatch(FilterText))
                {
                    list.Add(cacheDescriptor);
                }
            }

            CacheList.ItemsSource = null;
            CacheList.ItemsSource = list.OrderBy(c => c.Name);
            _listViewSorter.Resort();

            CacheList.ResizeColumn(NameColumn);
            CacheList.ResizeColumn(SizeColumn);
            CacheList.ResizeColumn(CountColumn);
            CacheList.ResizeColumn(MaxSizeColumn);
            CacheList.ResizeColumn(EnabledColumn);
            CacheList.ResizeColumn(ScavengableColumn);
            CacheList.ResizeColumn(RemainingSizeColumn);
            CacheList.ResizeColumn(PriorityColumn);

            if (_caches.Count > 0)
            {
                CacheList.SelectedIndex = 0;
            }
        }

        public class CacheDescriptor
        {
            public long Count { get; set; }

            public long Delta { get; set; }

            public bool Enabled { get; set; }

            [NotNull]
            public string FormattedCount
            {
                get { return Count.ToString("#,##0"); }
            }

            [NotNull]
            public string FormattedDelta
            {
                get { return Delta.ToString("#,##0 bytes"); }
            }

            [NotNull]
            public string FormattedMaxSize
            {
                get { return MaxSize.ToString("#,##0 bytes"); }
            }

            [NotNull]
            public string FormattedRemainingSize
            {
                get { return (MaxSize - Size).ToString("#,##0 bytes"); }
            }

            [NotNull]
            public string FormattedSize
            {
                get { return Size.ToString("#,##0 bytes"); }
            }

            public long MaxSize { get; set; }

            public string Name { get; set; }

            public string Priority { get; set; }

            public bool Scavengable { get; set; }

            public long Size { get; set; }
        }
    }
}
