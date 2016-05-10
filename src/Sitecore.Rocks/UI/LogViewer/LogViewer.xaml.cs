// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.DependencyObjectExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.LogViewer.Commands;

namespace Sitecore.Rocks.UI.LogViewer
{
    public partial class LogViewer : IContextProvider
    {
        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(@"IsRunning", typeof(bool), typeof(LogViewer));

        private readonly ListViewSorter _listViewSorter;

        public LogViewer()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(ListView);

            ExcludeFilterText = string.Empty;
            IncludeFilterText = string.Empty;
            Categories = string.Empty;

            Notifications.Unloaded += ControlUnloaded;
        }

        [NotNull]
        public string Categories { get; set; }

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }

            set
            {
                SetValue(IsRunningProperty, value);
                StartLog.IsChecked = value;
                PauseLog.IsChecked = !value;
            }
        }

        public int MaxItems { get; set; }

        [NotNull]
        public IPane Pane { get; set; }

        [CanBeNull]
        public Site Site { get; set; }

        public int UpdateSpeed { get; set; }

        [NotNull]
        protected string ExcludeFilterText { get; set; }

        [NotNull]
        protected string IncludeFilterText { get; set; }

        [CanBeNull]
        private Timer Timer { get; set; }

        [NotNull]
        public object GetContext()
        {
            return new LogViewerContext(this);
        }

        public void Initialize([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            SetSite(site);
        }

        public void LoadLog()
        {
            Stop();

            if (Site == null)
            {
                return;
            }

            if ((Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Logs) != DataServiceFeatureCapabilities.Logs)
            {
                return;
            }

            StartLog.IsEnabled = false;
            PauseLog.IsEnabled = false;
            RefreshLog.IsEnabled = false;

            LastUpdate.Text = Rocks.Resources.Loading;
            Site.DataService.GetLog(MaxItems, Categories, IncludeFilterText, ExcludeFilterText, GetLogCallback);
        }

        public void SetSite([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;
            Pane.Caption = @"Log Viewer [" + site.Name + "]";

            ListView.ItemsSource = null;
            IsRunning = true;
            LoadLog();
        }

        public void Stop()
        {
            if (Timer == null)
            {
                return;
            }

            Timer.Dispose();
            Timer = null;
        }

        protected override void OnInitialized([NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnInitialized(e);

            UpdateSpeed = 10;
            var value = AppHost.Settings.Get("Log Viewer", "Interval", "10") as string ?? string.Empty;

            int speed;
            if (int.TryParse(value, out speed))
            {
                UpdateSpeed = speed;
            }

            MaxItems = 25;
            value = AppHost.Settings.Get("Log Viewer", "Max Items", "25") as string ?? string.Empty;

            int maxItems;
            if (int.TryParse(value, out maxItems))
            {
                MaxItems = maxItems;
            }

            Categories = string.Empty;

            var categories = AppHost.Settings.Get("Log Viewer", "Categories", string.Empty);
            if (categories != null)
            {
                Categories = (string)categories;
            }

            ExcludeFilterText = AppHost.Settings.Get("Log Viewer", "Exclude Filter", string.Empty) as string ?? string.Empty;
            ExcludeFilter.Text = ExcludeFilterText;

            IncludeFilterText = AppHost.Settings.Get("Log Viewer", "Include Filter", string.Empty) as string ?? string.Empty;
            IncludeFilter.Text = IncludeFilterText;
        }

        private void ControlUnloaded([NotNull] object sender, [NotNull] object window)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(window, nameof(window));

            if (!this.IsContainedIn(window))
            {
                return;
            }

            Stop();
        }

        private void DoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var command = new ViewDetails();
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

            Stop();

            AppHost.Settings.Set("Log Viewer", "IncludeFilter", IncludeFilter.Text);
            AppHost.Settings.Set("Log Viewer", "ExcludeFilter", ExcludeFilter.Text);

            IncludeFilterText = IncludeFilter.Text;
            ExcludeFilterText = ExcludeFilter.Text;

            IsRunning = true;
            LoadLog();
        }

        private void GetLogCallback([NotNull] XDocument log)
        {
            Debug.ArgumentNotNull(log, nameof(log));

            var elements = log.XPathSelectElements(@"/rss/channel/item");

            var items = new List<LogItem>();

            foreach (var element in elements)
            {
                var logItem = new LogItem
                {
                    Title = element.GetElementValue("title"),
                    Description = element.GetElementValue("description"),
                    PublishDate = DateTimeExtensions.FromIso(element.GetElementValue("pubDate")),
                    Category = element.GetElementValue("category")
                };

                items.Add(logItem);
            }

            Dispatcher.Invoke(() => LoadListView(items));
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            _listViewSorter.HeaderClick(sender, e);
        }

        private void LoadListView([NotNull] List<LogItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            ListView.ItemsSource = items;

            LastUpdate.Text = DateTime.Now.ToString(CultureInfo.CurrentCulture);

            StartLog.IsEnabled = true;
            PauseLog.IsEnabled = true;
            RefreshLog.IsEnabled = true;

            if (IsRunning)
            {
                Start();
            }
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var context = GetContext();

            var commands = Rocks.Commands.CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }

        private void OpenMenu([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            var contextMenu = ContextMenuExtensions.GetContextMenu(context);
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = Menu;
            contextMenu.IsOpen = true;
        }

        private void PauseClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Stop();
            IsRunning = false;
        }

        private void RefreshClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Stop();
            LoadLog();
        }

        private void Start()
        {
            Timer = new Timer(Tick, null, new TimeSpan(0, 0, UpdateSpeed), new TimeSpan(0, 0, 0, 0, -1));
        }

        private void StartClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Stop();

            IsRunning = true;
            LoadLog();
        }

        private void Tick([CanBeNull] object state)
        {
            Dispatcher.Invoke(LoadLog);
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }
    }
}
