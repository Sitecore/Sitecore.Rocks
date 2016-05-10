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
using Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes
{
    [Management(ItemName, 5500)]
    public partial class IndexViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Indexes";

        private readonly List<DocumentFieldDescriptor> documentFields = new List<DocumentFieldDescriptor>();

        private readonly ListViewSorter documentListSorter;

        private readonly ListViewSorter fieldListSorter;

        private readonly List<IndexDescriptor> indexes = new List<IndexDescriptor>();

        private readonly ListViewSorter indexListSorter;

        public IndexViewer()
        {
            InitializeComponent();

            indexListSorter = new ListViewSorter(IndexList);
            fieldListSorter = new ListViewSorter(FieldsList);
            documentListSorter = new ListViewSorter(DocumentList);

            FilterText = string.Empty;

            Loaded += ControlLoaded;

            EnableButtons();
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
            SearchDataGrid.Site = Context.Site;

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

        private void DocumentListHeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            documentListSorter.HeaderClick(sender, e);
        }

        private void EnableButtons()
        {
            var indexDescriptor = IndexList.SelectedItem as IndexDescriptor;

            long index;
            long.TryParse(DocumentNo.Text, out index);

            PreviousButton.IsEnabled = indexDescriptor != null && index > 0;
            NextButton.IsEnabled = indexDescriptor != null && index < indexDescriptor.Count;
            SearchButton.IsEnabled = indexDescriptor != null;
        }

        private void ExploreTerms([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            var context = (IndexViewerContext)GetContext();
            context.ClickTarget = IndexViewerContext.FieldList;

            var command = new ExploreTerms();
            if (command.CanExecute(context))
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            }
        }

        private void FieldsListHeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            fieldListSorter.HeaderClick(sender, e);
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderIndexes();
        }

        private void HandleDocumentIndexKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Enter)
            {
                LoadDocument();
                EnableButtons();
            }
        }

        private void HandleSearchKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Enter)
            {
                LoadSearch();
            }
        }

        private void IndexListHeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            indexListSorter.HeaderClick(sender, e);
        }

        private void LoadDocument()
        {
            var index = IndexList.SelectedItem as IndexDescriptor;
            if (index == null)
            {
                return;
            }

            long documentNo;
            long.TryParse(DocumentNo.Text, out documentNo);

            if (documentNo > index.Count - 1)
            {
                documentNo = index.Count - 1;
                DocumentNo.Text = documentNo.ToString();
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseDocument(root);

                RenderDocument();
            };

            DocumentList.ItemsSource = null;

            Context.Site.DataService.ExecuteAsync("Indexes.GetDocument", completed, index.Name, documentNo.ToString());
        }

        private void LoadSearch([NotNull] string fieldName, [NotNull] string searchText, [NotNull] string type, int offset)
        {
            Debug.ArgumentNotNull(fieldName, nameof(fieldName));
            Debug.ArgumentNotNull(searchText, nameof(searchText));
            Debug.ArgumentNotNull(type, nameof(type));

            var index = IndexList.SelectedItem as IndexDescriptor;
            if (index == null)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                SearchDataGrid.Load(root, offset);
                SearchDataGrid.Visibility = Visibility.Visible;
            };

            SearchDataGrid.Clear();

            Context.Site.DataService.ExecuteAsync("Indexes.Search", completed, index.Name, fieldName, searchText, type, offset.ToString());
        }

        private void LoadSearch(int offset = 0)
        {
            var t = QueryType.SelectedItem as ComboBoxItem;
            if (t == null)
            {
                return;
            }

            var fieldName = FieldName.Text;
            var searchText = SearchText.Text;
            var type = t.Tag as string ?? string.Empty;

            LoadSearch(fieldName, searchText, type, offset);
        }

        private void NextDocument([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var indexDescriptor = IndexList.SelectedItem as IndexDescriptor;
            if (indexDescriptor == null)
            {
                return;
            }

            long index;
            if (long.TryParse(DocumentNo.Text, out index))
            {
                if (index < indexDescriptor.Count - 1)
                {
                    index++;
                }
            }

            DocumentNo.Text = index.ToString();

            LoadDocument();
            EnableButtons();
        }

        private void OpenFieldListContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = (IndexViewerContext)GetContext();

            context.ClickTarget = IndexViewerContext.FieldList;

            FieldsListPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void OpenIndexListContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = (IndexViewerContext)GetContext();

            context.ClickTarget = IndexViewerContext.IndexList;

            IndexListPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseDocument([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            documentFields.Clear();

            foreach (var element in root.Elements())
            {
                var field = new DocumentFieldDescriptor
                {
                    Name = element.GetAttributeValue("name"),
                    Value = element.Value
                };

                documentFields.Add(field);
            }
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

            indexes.Clear();
            indexes.AddRange(list);
        }

        private void PreviousDocument([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            long index;
            long.TryParse(DocumentNo.Text, out index);

            if (index > 0)
            {
                index--;
            }

            DocumentNo.Text = index.ToString();

            LoadDocument();
            EnableButtons();
        }

        private void RenderDocument()
        {
            DocumentList.ItemsSource = documentFields;

            IndexList.ResizeColumn(DocumentFieldNameColumn);
            IndexList.ResizeColumn(DocumentFieldValueColumn);

            DocumentList.Visibility = Visibility.Visible;

            documentListSorter.Resort();
        }

        private void RenderIndexes()
        {
            var list = new List<IndexDescriptor>();
            foreach (var indexDescriptor in indexes)
            {
                if (indexDescriptor.Name.IsFilterMatch(FilterText))
                {
                    list.Add(indexDescriptor);
                }
            }

            IndexList.ItemsSource = null;
            IndexList.ItemsSource = list.OrderBy(c => c.Name);
            indexListSorter.Resort();

            IndexList.ResizeColumn(NameColumn);
            IndexList.ResizeColumn(CountColumn);

            if (indexes.Count > 0)
            {
                IndexList.SelectedIndex = 0;
            }

            EnableButtons();
        }

        private void Search([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LoadSearch();
        }

        private void SearchPageChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LoadSearch(SearchDataGrid.Offset);
        }

        private void SetIndexSelection([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var indexDescriptor = IndexList.SelectedItem as IndexDescriptor;
            if (indexDescriptor == null)
            {
                return;
            }

            FieldsList.ItemsSource = indexDescriptor.Fields.OrderBy(i => i.Name);
            fieldListSorter.Resort();

            DocumentList.Visibility = Visibility.Collapsed;
            DocumentList.ItemsSource = null;
            DocumentNo.Text = string.Empty;
            DocumentIndexMax.Text = " of " + (indexDescriptor.Count - 1).ToString("#,##0");
            documentFields.Clear();

            SearchDataGrid.Clear();
            SearchDataGrid.Visibility = Visibility.Collapsed;

            EnableButtons();
        }

        public class DocumentFieldDescriptor
        {
            public string Name { get; set; }

            public string Value { get; set; }
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
