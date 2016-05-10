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

namespace Sitecore.Rocks.UI.Management.ManagementItems.History
{
    [Management(ItemName, 2000)]
    public partial class HistoryViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "History";

        private readonly List<HistoryEntry> history = new List<HistoryEntry>();

        private readonly ListViewSorter listViewSorter;

        public HistoryViewer()
        {
            InitializeComponent();

            FilterText = string.Empty;

            listViewSorter = new ListViewSorter(History);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseManagementContext Context { get; set; }

        [NotNull]
        protected string FilterText { get; set; }

        protected bool IsLoading { get; set; }

        public bool CanExecute(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return context is DatabaseManagementContext;
        }

        [NotNull]
        public object GetContext()
        {
            return new HistoryViewerContext(this);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (DatabaseManagementContext)context;

            return this;
        }

        public void Refresh()
        {
            if (IsLoading)
            {
                return;
            }

            Loading.ShowLoading(History);

            History.ItemsSource = null;

            var fromDate = DateTimeExtensions.ToIsoDate(FromDateTimeBox.Value ?? DateTime.UtcNow);
            var toDate = DateTimeExtensions.ToIsoDate(ToDateTimeBox.Value ?? DateTime.UtcNow);

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.HideLoading(History);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseHistory(root);

                RenderHistory();
            };

            Loading.ShowLoading(History);

            Context.DatabaseUri.Site.DataService.ExecuteAsync("Items.GetHistory", callback, Context.DatabaseUri.DatabaseName.ToString(), fromDate, toDate, string.Empty, string.Empty);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            IsLoading = true;
            try
            {
                FromDateTimeBox.Value = DateTime.UtcNow.AddDays(-1);
                ToDateTimeBox.Value = DateTime.UtcNow;
            }
            finally
            {
                IsLoading = false;
            }

            Refresh();
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderHistory();
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

        private void ParseHistory([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            history.Clear();

            foreach (var element in root.Elements())
            {
                var created = DateTimeExtensions.FromIso(element.GetAttributeValue("created"));

                var itemId = new ItemId(new Guid(element.GetAttributeValue("id")));
                var language = new Language(element.GetAttributeValue("language"));
                var version = new Data.Version(element.GetAttributeInt("version", Data.Version.Latest.Number));

                var entry = new HistoryEntry(Context.DatabaseUri)
                {
                    Action = element.GetAttributeValue("action"),
                    Category = element.GetAttributeValue("category"),
                    Created = created,
                    ItemId = itemId,
                    Language = language,
                    Version = version,
                    Task = element.GetAttributeValue("task"),
                    UserName = element.GetAttributeValue("username"),
                    Path = element.GetAttributeValue("path"),
                    AdditionalInfo = element.Value
                };

                history.Add(entry);
            }
        }

        private void RenderHistory()
        {
            var list = history.Where(entry => entry.Action.IsFilterMatch(FilterText) || entry.UserName.IsFilterMatch(FilterText) || entry.Task.IsFilterMatch(FilterText) || entry.AdditionalInfo.IsFilterMatch(FilterText)).ToList();

            History.ItemsSource = null;
            History.ItemsSource = list;
            listViewSorter.Resort();

            ResizeGridViewColumn(ActionColumn);
            ResizeGridViewColumn(CategoryColumn);
            ResizeGridViewColumn(CreatedColumn);
            ResizeGridViewColumn(ItemIdColumn);
            ResizeGridViewColumn(LanguageColumn);
            ResizeGridViewColumn(VersionColumn);
            ResizeGridViewColumn(UserNameColumn);
            ResizeGridViewColumn(TaskColumn);
            ResizeGridViewColumn(PathColumn);
            ResizeGridViewColumn(AdditionalInfoColumn);

            if (history.Count > 0)
            {
                History.SelectedIndex = 0;
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

        private void ValueChanged(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public class HistoryEntry : IItem
        {
            public HistoryEntry(DatabaseUri databaseUri)
            {
                DatabaseUri = databaseUri;
            }

            public string Action { get; set; }

            public string AdditionalInfo { get; set; }

            public string Category { get; set; }

            public DateTime Created { get; set; }

            public DatabaseUri DatabaseUri { get; set; }

            [NotNull]
            public string FormattedCreated
            {
                get
                {
                    if (Created == DateTime.MinValue)
                    {
                        return string.Empty;
                    }

                    return Created.ToString();
                }
            }

            public ItemId ItemId { get; set; }

            public Language Language { get; set; }

            public string Path { get; set; }

            public string Task { get; set; }

            public string UserName { get; set; }

            public Data.Version Version { get; set; }

            Icon IItem.Icon
            {
                get { return Icon.Empty; }
            }

            ItemUri IItemUri.ItemUri
            {
                get { return new ItemUri(DatabaseUri, ItemId); }
            }

            string IItem.Name
            {
                get
                {
                    var n = Path.LastIndexOf('/');
                    if (n < 0)
                    {
                        return string.Empty;
                    }

                    return Path.Mid(n + 1);
                }
            }
        }
    }
}
