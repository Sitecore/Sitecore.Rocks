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
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.LanguageViewers
{
    [Management(ItemName, 1000)]
    public partial class LanguageViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Languages";

        private readonly List<LanguageDescriptor> languages = new List<LanguageDescriptor>();

        private readonly ListViewSorter listViewSorter;

        public LanguageViewer()
        {
            InitializeComponent();

            listViewSorter = new ListViewSorter(LanguageList);
            FilterText = string.Empty;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseManagementContext Context { get; set; }

        [NotNull]
        protected string FilterText { get; set; }

        public bool CanExecute(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return context is DatabaseManagementContext;
        }

        [NotNull]
        public object GetContext()
        {
            return new LanguageViewerContext(this);
        }

        public UIElement GetControl([NotNull] IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (DatabaseManagementContext)context;

            return this;
        }

        public void LoadLanguages()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.Swap(LanguageList);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseLanguages(root);
                RenderLanguages();
            };

            Loading.ShowLoading(LanguageList);

            Context.DatabaseUri.Site.DataService.ExecuteAsync("Languages.GetLanguages", callback, Context.DatabaseUri.DatabaseName.ToString());
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadLanguages();
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderLanguages();
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = AppHost.ContextMenus.Build(GetContext(), e);
        }

        private void ParseLanguages([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            var list = new List<LanguageDescriptor>();

            foreach (var element in root.Elements())
            {
                var icon = new Icon(Context.DatabaseUri.Site, element.GetAttributeValue("icon"));

                var itemUri = ItemUri.Empty;

                var id = element.GetAttributeValue("itemid");
                Guid guid;
                if (Guid.TryParse(id, out guid))
                {
                    itemUri = new ItemUri(Context.DatabaseUri, new ItemId(guid));
                }

                var languageDescriptor = new LanguageDescriptor
                {
                    Name = element.GetAttributeValue("name"),
                    DisplayName = element.GetAttributeValue("displayname"),
                    ItemUri = itemUri,
                    Icon = icon
                };

                list.Add(languageDescriptor);
            }

            languages.Clear();
            languages.AddRange(list);
        }

        private void RenderLanguages()
        {
            var list = new List<LanguageDescriptor>();
            foreach (var statisticsDescriptor in languages)
            {
                if (statisticsDescriptor.Name.IsFilterMatch(FilterText))
                {
                    list.Add(statisticsDescriptor);
                }
            }

            LanguageList.ItemsSource = null;
            LanguageList.ItemsSource = list.OrderBy(c => c.Name);
            listViewSorter.Resort();

            ResizeGridViewColumn(NameColumn);
            ResizeGridViewColumn(DisplayNameColumn);

            if (languages.Count > 0)
            {
                LanguageList.SelectedIndex = 0;
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

        public class LanguageDescriptor
        {
            public string DisplayName { get; set; }

            public Icon Icon { get; set; }

            public ItemUri ItemUri { get; set; }

            public string Name { get; set; }
        }
    }
}
