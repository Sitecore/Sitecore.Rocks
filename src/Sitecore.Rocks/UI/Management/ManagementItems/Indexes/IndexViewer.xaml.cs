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
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes
{
    [Management(ItemName, 5500)]
    public partial class IndexViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Indexes";

        private readonly List<IndexDescriptor> _indexes = new List<IndexDescriptor>();

        private readonly ListViewSorter _indexListSorter;

        public IndexViewer()
        {
            InitializeComponent();

            _indexListSorter = new ListViewSorter(IndexList);

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

            return site.Site.DataService.CanExecuteAsync("Indexes.GetIndexes");
        }

        [NotNull]
        public object GetContext()
        {
            return new IndexViewerContext(this);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (SiteManagementContext)context;

            return this;
        }

        [CanBeNull]
        public IndexDescriptor GetSelectedIndex()
        {
            return IndexList.SelectedItem as IndexDescriptor;
        }

        public void LoadIndexes()
        {
            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                Loading.Swap(IndexList);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseIndexes(root);
                RenderIndexes();
            };

            Loading.ShowLoading(IndexList);

            Context.Site.DataService.ExecuteAsync("Indexes.GetIndexes", completed);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadIndexes();
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderIndexes();
        }

        private void IndexListHeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            _indexListSorter.HeaderClick(sender, e);
        }

        private void OpenIndexListContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = (IndexViewerContext)GetContext();

            context.ClickTarget = IndexViewerContext.IndexList;

            IndexListPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseIndexes([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            var list = new List<IndexDescriptor>();

            foreach (var element in root.Elements())
            {
                var indexDescriptor = new IndexDescriptor
                {
                    Name = element.GetAttributeValue("name"),
                    Count = element.GetAttributeLong("count", 0)
                };

                var fields = element.Element("fields");
                if (fields != null)
                {
                    foreach (var field in fields.Elements())
                    {
                        var fieldDescriptor = new IndexFieldDescriptor
                        {
                            Name = field.GetAttributeValue("name"),
                            Count = field.GetAttributeInt("count", 0)
                        };

                        indexDescriptor.Fields.Add(fieldDescriptor);
                    }
                }

                list.Add(indexDescriptor);
            }

            _indexes.Clear();
            _indexes.AddRange(list);
        }

        private void RenderIndexes()
        {
            var list = new List<IndexDescriptor>();
            foreach (var indexDescriptor in _indexes)
            {
                if (indexDescriptor.Name.IsFilterMatch(FilterText))
                {
                    list.Add(indexDescriptor);
                }
            }

            IndexList.ItemsSource = null;
            IndexList.ItemsSource = list.OrderBy(c => c.Name);
            _indexListSorter.Resort();

            IndexList.ResizeColumn(NameColumn);

            if (_indexes.Count > 0)
            {
                IndexList.SelectedIndex = 0;
            }
        }

        public class IndexDescriptor
        {
            public IndexDescriptor()
            {
                Fields = new List<IndexFieldDescriptor>();
            }

            public long Count { get; set; }

            public List<IndexFieldDescriptor> Fields { get; set; }

            [NotNull]
            public string FormattedCount
            {
                get { return Count.ToString("#,##0"); }
            }

            public string Name { get; set; }
        }

        public class IndexFieldDescriptor
        {
            public int Count { get; set; }

            [NotNull]
            public string FormattedCount
            {
                get { return Count.ToString("#,##0"); }
            }

            public string Name { get; set; }
        }
    }
}
