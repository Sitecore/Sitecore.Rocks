// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.DependencyObjectExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.JobViewer
{
    public partial class JobViewer : IContextProvider
    {
        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(@"IsRunning", typeof(bool), typeof(JobViewer));

        private readonly ListViewSorter _listViewSorter;

        public JobViewer()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(ListView);

            Notifications.Unloaded += ControlUnloaded;
        }

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

        [NotNull]
        public IPane Pane { get; set; }

        [CanBeNull]
        public Site Site { get; set; }

        public int UpdateSpeed { get; set; }

        protected List<JobItem> Items { get; set; }

        private Timer Timer { get; set; }

        [NotNull]
        public object GetContext()
        {
            return new JobViewerContext(this);
        }

        public void Initialize([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            SetSite(site);
        }

        public void LoadJobs()
        {
            Stop();

            if (Site == null)
            {
                return;
            }

            if ((Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Jobs) != DataServiceFeatureCapabilities.Jobs)
            {
                return;
            }

            StartLog.IsEnabled = false;
            PauseLog.IsEnabled = false;
            RefreshLog.IsEnabled = false;

            LastUpdate.Text = Rocks.Resources.JobViewer_LoadJobs_Loading___;
            Site.DataService.GetJobs(GetJobsCompleted);
        }

        public void SetSite([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;
            Pane.Caption = @"Job Viewer [" + site.Name + "]";

            ListView.ItemsSource = null;
            IsRunning = true;
            LoadJobs();
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

            InitializeUpdateSpeed();
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

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var items = Items ?? new List<JobItem>();
            LoadListView(items);
        }

        private void GetJobsCompleted([NotNull] XDocument jobs)
        {
            Debug.ArgumentNotNull(jobs, nameof(jobs));

            var elements = jobs.XPathSelectElements(@"/jobs/job");

            var items = new List<JobItem>();

            foreach (var element in elements)
            {
                var jobItem = new JobItem
                {
                    Name = element.GetAttributeValue("name"),
                    Failed = element.GetAttributeValue("failed") == @"1",
                    QueueTime = DateTimeExtensions.FromIso(element.GetAttributeValue("queuetime")),
                    IsDone = element.GetAttributeValue("isdone") == @"1",
                    Processed = int.Parse(element.GetAttributeValue("processed")),
                    Total = int.Parse(element.GetAttributeValue("total")),
                    State = element.GetAttributeValue("state"),
                    Category = element.GetAttributeValue("category")
                };

                items.Add(jobItem);
            }

            Dispatcher.Invoke(() => LoadListView(items));
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            _listViewSorter.HeaderClick(sender, e);
        }

        private void InitializeUpdateSpeed()
        {
            UpdateSpeed = 10;
            var value = AppHost.Settings.Get("Job Viewer", "Interval", 10);
            if (value != null)
            {
                if (value is string)
                {
                    int speed;
                    if (!int.TryParse((string)value, out speed))
                    {
                        UpdateSpeed = speed;
                    }
                }
                else
                {
                    UpdateSpeed = (int)value;
                }
            }
        }

        private void LoadListView([NotNull] List<JobItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            Items = items;

            var list = new List<JobItem>();

            foreach (var jobItem in items)
            {
                if (jobItem.Name.IsFilterMatch(IncludeFilter.Text))
                {
                    list.Add(jobItem);
                }
            }

            ListView.ItemsSource = list;

            _listViewSorter.Resort();

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

            var commands = CommandManager.GetCommands(context).ToList();
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

        private void PauseJobsClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Stop();
            IsRunning = false;
        }

        private void RefreshJobsClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Stop();
            LoadJobs();
        }

        private void Start()
        {
            Timer = new Timer(Tick, null, new TimeSpan(0, 0, UpdateSpeed), new TimeSpan(0, 0, 0, 0, -1));
        }

        private void StartJobsClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Stop();

            IsRunning = true;
            LoadJobs();
        }

        private void Tick([CanBeNull] object state)
        {
            Dispatcher.Invoke(LoadJobs);
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }
    }
}
