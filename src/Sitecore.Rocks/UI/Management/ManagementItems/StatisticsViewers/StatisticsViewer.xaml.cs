// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.StatisticsViewers
{
    [Management(ItemName, 6000)]
    public partial class StatisticsViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Statistics";

        private readonly ListViewSorter listViewSorter;

        private readonly List<StatisticsDescriptor> statistics = new List<StatisticsDescriptor>();

        public StatisticsViewer()
        {
            InitializeComponent();

            listViewSorter = new ListViewSorter(StatisticsList);
            FilterText = string.Empty;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public SiteManagementContext Context { get; set; }

        [NotNull]
        protected string FilterText { get; set; }

        public bool CanExecute(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var site = context as SiteManagementContext;
            if (site == null)
            {
                return false;
            }

            return site.Site.DataService.CanExecuteAsync("Statistics.GetStatistics");
        }

        [NotNull]
        public object GetContext()
        {
            return new StatisticsViewerContext(this);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (SiteManagementContext)context;

            return this;
        }

        public void LoadStatistics()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.Swap(StatisticsList);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseStatistics(root);
                RenderStatistics();
            };

            Loading.ShowLoading(StatisticsList);

            Context.Site.DataService.ExecuteAsync("Statistics.GetStatistics", callback);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadStatistics();
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderStatistics();
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

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseStatistics([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            var list = new List<StatisticsDescriptor>();

            foreach (var element in root.Elements())
            {
                var statisticsDescriptor = new StatisticsDescriptor
                {
                    Name = element.GetAttributeValue("name"),
                    SiteName = element.GetAttributeValue("site"),
                    RenderCount = element.GetAttributeLong("rendercount", 0),
                    UsedCache = element.GetAttributeLong("usedcache", 0),
                    AverageTime = element.GetAttributeDouble("averagetime", 0),
                    AverageItems = element.GetAttributeLong("averageitems", 0),
                    MaxTime = element.GetAttributeDouble("maxtime", 0),
                    MaxItemsAccessed = element.GetAttributeLong("maxitemsaccessed", 0),
                    TotalTime = element.GetAttributeDouble("totaltime", 0),
                    TotalItemsAccessed = element.GetAttributeLong("totalitems", 0),
                    LastRendered = DateTimeExtensions.FromIso(element.GetAttributeValue("lastrendered")),
                };

                list.Add(statisticsDescriptor);
            }

            statistics.Clear();
            statistics.AddRange(list);
        }

        private void RenderStatistics()
        {
            var list = new List<StatisticsDescriptor>();
            foreach (var statisticsDescriptor in statistics)
            {
                if (statisticsDescriptor.Name.IsFilterMatch(FilterText))
                {
                    list.Add(statisticsDescriptor);
                }
            }

            StatisticsList.ItemsSource = null;
            StatisticsList.ItemsSource = list.OrderBy(c => c.Name);
            listViewSorter.Resort();

            ResizeGridViewColumn(NameColumn);
            ResizeGridViewColumn(SiteNameColumn);
            ResizeGridViewColumn(RenderCountColumn);
            ResizeGridViewColumn(UsedCacheColumn);
            ResizeGridViewColumn(AverageTimeColumn);
            ResizeGridViewColumn(AverageItemsColumn);
            ResizeGridViewColumn(MaxTimeColumn);
            ResizeGridViewColumn(MaxItemsColumn);
            ResizeGridViewColumn(TotalTimeColumn);
            ResizeGridViewColumn(TotalItemsColumn);
            ResizeGridViewColumn(LastRenderedColumn);

            if (statistics.Count > 0)
            {
                StatisticsList.SelectedIndex = 0;
            }
        }

        private void ResizeGridViewColumn([NotNull] GridViewColumn column)
        {
            Debug.ArgumentNotNull(column, nameof(column));

            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }

        public class StatisticsDescriptor
        {
            public long AverageItems { get; set; }

            public double AverageTime { get; set; }

            [NotNull]
            public string FormattedAverageItems
            {
                get { return AverageItems.ToString("#,##0"); }
            }

            [NotNull]
            public string FormattedAverageTime
            {
                get { return AverageTime.ToString("#,##0.00 ms"); }
            }

            [NotNull]
            public string FormattedLastRendered
            {
                get { return LastRendered.ToString(); }
            }

            [NotNull]
            public string FormattedMaxItemsAccessed
            {
                get { return MaxItemsAccessed.ToString("#,##0"); }
            }

            [NotNull]
            public string FormattedMaxTime
            {
                get { return MaxTime.ToString("#,##0.00 ms"); }
            }

            [NotNull]
            public string FormattedRenderCount
            {
                get { return RenderCount.ToString("#,##0"); }
            }

            [NotNull]
            public string FormattedTotalItemsAccessed
            {
                get { return TotalItemsAccessed.ToString("#,##0"); }
            }

            [NotNull]
            public string FormattedTotalTime
            {
                get { return TotalTime.ToString("#,##0.00 ms"); }
            }

            [NotNull]
            public string FormattedUsedCache
            {
                get { return UsedCache.ToString("#,##0 bytes"); }
            }

            public DateTime LastRendered { get; set; }

            public long MaxItemsAccessed { get; set; }

            public double MaxTime { get; set; }

            public string Name { get; set; }

            public long RenderCount { get; set; }

            public string SiteName { get; set; }

            public long TotalItemsAccessed { get; set; }

            public double TotalTime { get; set; }

            public long UsedCache { get; set; }
        }
    }
}
