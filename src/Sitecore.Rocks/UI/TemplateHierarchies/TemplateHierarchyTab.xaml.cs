// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Commands.Editing;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.TemplateHierarchies.Items;

namespace Sitecore.Rocks.UI.TemplateHierarchies
{
    public partial class TemplateHierarchyTab : IContextProvider
    {
        public static readonly DependencyProperty IsSubTemplatesProperty = DependencyProperty.Register(@"IsSubTemplates", typeof(bool), typeof(TemplateHierarchyTab));

        public TemplateHierarchyTab()
        {
            InitializeComponent();

            IsSubTemplates = true;

            ItemTreeView.SelectedItemsChanged += UpdateFields;
            ItemTreeView.GetContext = GetItemTreeViewContext;
        }

        public bool IsSubTemplates
        {
            get { return (bool)GetValue(IsSubTemplatesProperty); }

            set
            {
                SetValue(IsSubTemplatesProperty, value);

                SubTemplatesButton.IsChecked = value;
                SuperTemplatesButton.IsChecked = !value;
            }
        }

        [NotNull]
        public TabItem TabItem { get; set; }

        [NotNull]
        public TemplateHierarchyViewer TemplateHierarchyViewer { get; set; }

        [NotNull]
        public ItemUri TemplateUri { get; set; }

        [NotNull]
        public object GetContext()
        {
            var result = new TemplateHierarchyFieldsContext(this);

            return result;
        }

        public void Initialize([NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            TemplateUri = templateUri;

            LoadHierarchy();
        }

        private void DoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var command = new EditItems();

            var context = GetContext();

            if (!command.CanExecute(context))
            {
                return;
            }

            AppHost.Usage.ReportCommand(command, context);
            command.Execute(context);
            e.Handled = true;
        }

        [CanBeNull]
        private object GetItemTreeViewContext([CanBeNull] object source)
        {
            return new TemplateHierarchyItemTreeViewContext(ItemTreeView, ItemTreeView.TreeView.GetSelectedItems(source), this);
        }

        private void GotoTemplate([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItems = ItemTreeView.SelectedItems;

            if (selectedItems.Count != 1)
            {
                return;
            }

            var item = selectedItems[0] as TemplateHierarchyTreeViewItem;
            if (item == null)
            {
                return;
            }

            TemplateUri = item.ItemUri;

            LoadHierarchy();
        }

        private void LoadFields([NotNull] ItemUri templateUri)
        {
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));

            GetValueCompleted<XDocument> completed = delegate(XDocument doc)
            {
                Debug.ArgumentNotNull(doc, nameof(doc));

                FieldsLoading.HideLoading(FieldsListViewBorder, FieldsTextPanel);

                var templateElement = doc.XPathSelectElement(@"/template");
                if (templateElement != null)
                {
                    RenderFields(templateElement);
                }
            };

            FieldsLoading.ShowLoading(FieldsListViewBorder, FieldsTextPanel);

            templateUri.Site.DataService.GetTemplateXml(templateUri, false, completed);
        }

        private void LoadHierarchy()
        {
            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                FieldsListViewBorder.Visibility = Visibility.Visible;
                Loading.HideLoading(ItemTreeView);

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var header = new TabItemHeader
                {
                    Header = root.GetAttributeValue("name"),
                    Tag = this
                };

                header.Click += delegate { TemplateHierarchyViewer.CloseTab(TabItem); };

                TabItem.Header = header;

                RenderHierarchy(root);

                ItemTreeView.TreeView.Clear();
                if (ItemTreeView.TreeView.Items.Count <= 0)
                {
                    return;
                }

                var treeViewItem = ItemTreeView.TreeView.Items[0] as BaseTreeViewItem;
                if (treeViewItem == null)
                {
                    return;
                }

                treeViewItem.IsSelected = true;
                treeViewItem.IsItemSelected = true;
                Keyboard.Focus(treeViewItem);
            };

            FieldsLoading.Visibility = Visibility.Collapsed;
            FieldsListViewBorder.Visibility = Visibility.Collapsed;
            FieldsTextPanel.Visibility = Visibility.Collapsed;
            FieldsListView.Items.Clear();

            Loading.ShowLoading(ItemTreeView);
            TemplateUri.Site.DataService.ExecuteAsync("Templates.GetHierarchy", completed, TemplateUri.DatabaseName.ToString(), TemplateUri.ItemId.ToString(), IsSubTemplates ? "SubTemplates" : "SuperTemplates");
        }

        private void OpenFieldsContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FieldsListViewBorder.ContextMenu = AppHost.ContextMenus.Build(GetContext(), e);
        }

        private void Refresh([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LoadHierarchy();
        }

        private void RenderFields([NotNull] XElement templateElement)
        {
            Debug.ArgumentNotNull(templateElement, nameof(templateElement));

            FieldsListView.Items.Clear();

            foreach (var sectionElement in templateElement.Elements())
            {
                var section = new TemplateElement
                {
                    Name = sectionElement.GetAttributeValue("name"),
                    Type = string.Empty,
                    ItemUri = new ItemUri(TemplateUri.DatabaseUri, new ItemId(new Guid(sectionElement.GetAttributeValue("id")))),
                    Icon = new Icon(TemplateUri.Site, sectionElement.GetAttributeValue("icon"))
                };

                FieldsListView.Items.Add(section);

                foreach (var fieldElement in sectionElement.Elements())
                {
                    var field = new TemplateElement
                    {
                        Name = fieldElement.GetAttributeValue("name"),
                        Type = fieldElement.GetAttributeValue("type"),
                        Unversioned = fieldElement.GetAttributeValue("unversioned") == "1",
                        Shared = fieldElement.GetAttributeValue("shared") == "1",
                        Source = fieldElement.GetAttributeValue("source"),
                        ItemUri = new ItemUri(TemplateUri.DatabaseUri, new ItemId(new Guid(fieldElement.GetAttributeValue("id")))),
                        Icon = new Icon(TemplateUri.Site, sectionElement.GetAttributeValue("icon"))
                    };

                    if (string.IsNullOrEmpty(field.Type))
                    {
                        field.Type = "Single-Line Text";
                    }

                    FieldsListView.Items.Add(field);
                }
            }

            FieldsListView.ResizeColumn(NameColumn);
            FieldsListView.ResizeColumn(TypeColumn);
            FieldsListView.ResizeColumn(VersioningColumn);
            FieldsListView.ResizeColumn(SourceColumn);

            if (FieldsListView.Items.Count == 0)
            {
                FieldsTextPanel.Visibility = Visibility.Visible;
                FieldsListViewBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void RenderHierarchy([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            ItemTreeView.TreeView.Clear();
            ItemTreeView.TreeView.Items.Clear();

            var item = new TemplateHierarchyTreeViewItem(root, TemplateUri.DatabaseUri);

            ItemTreeView.TreeView.Items.Add(item);

            item.IsExpanded = true;
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }

        private void UpdateFields([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItems = ItemTreeView.SelectedItems;

            if (selectedItems.Count != 1)
            {
                FieldsListView.Items.Clear();
                return;
            }

            var item = selectedItems[0] as TemplateHierarchyTreeViewItem;
            if (item == null)
            {
                FieldsListView.Items.Clear();
                return;
            }

            LoadFields(item.ItemUri);
        }

        private void ViewSubTemplates([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsSubTemplates = true;
            LoadHierarchy();
        }

        private void ViewSuperTemplates([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsSubTemplates = false;
            LoadHierarchy();
        }

        public class TemplateElement : IItem
        {
            [NotNull]
            public string FormattedName
            {
                get
                {
                    if (!string.IsNullOrEmpty(Type))
                    {
                        return "    " + Name;
                    }

                    return Name;
                }
            }

            public Icon Icon { get; set; }

            public ItemUri ItemUri { get; set; }

            public string Name { get; set; }

            public bool Shared { get; set; }

            [NotNull]
            public string Source { get; set; }

            [NotNull]
            public string Type { get; set; }

            public bool Unversioned { get; set; }

            [NotNull]
            public string Versioning
            {
                get
                {
                    if (string.IsNullOrEmpty(Type))
                    {
                        return string.Empty;
                    }

                    if (Shared)
                    {
                        return "Shared";
                    }

                    if (Unversioned)
                    {
                        return "Unversioned";
                    }

                    return "Versioned";
                }
            }
        }
    }
}
