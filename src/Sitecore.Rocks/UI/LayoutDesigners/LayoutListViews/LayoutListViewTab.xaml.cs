// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Extensions.XmlTextWriterExtensions;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectColumnsDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Extensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.PlaceHolders;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews
{
    // <r xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    // <d id="{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}" l="{14030E9F-CE92-49C6-AD87-7D49B50E42EA}">
    // <r ds="" id="{885B8314-7D8C-4CBB-8000-01421EA8F406}" par="" ph="main" uid="{43222D12-08C9-453B-AE96-D406EBB95126}" />
    // <r ds="" id="{CE4ADCFB-7990-4980-83FB-A00C1E3673DB}" par="" ph="/main/centercolumn" uid="{CF044AD9-0332-407A-ABDE-587214A2C808}" />
    // <r ds="" id="{493B3A83-0FA7-4484-8FC9-4680991CF743}" par="" ph="/main/centercolumn/content" uid="{B343725A-3A93-446E-A9C8-3A2CBD3DB489}" />
    // </d>
    // <d id="{46D2F427-4CE5-4E1F-BA10-EF3636F43534}" l="{14030E9F-CE92-49C6-AD87-7D49B50E42EA}">
    // <r ds="" id="{493B3A83-0FA7-4484-8FC9-4680991CF743}" par="" ph="content" uid="{A08C9132-DBD1-474F-A2CA-6CA26A4AA650}" />
    // </d>
    // </r>

    public partial class LayoutListViewTab : IContextProvider, IRenderingContainer
    {
        private const string DefaultColumns = "ID,Rendering,Placeholder,Data Source";

        [NotNull]
        private readonly ObservableCollection<LayoutDesignerItem> items = new ObservableCollection<LayoutDesignerItem>();

        private readonly ListViewAdorner listViewAdorner;

        private Point dragOrigin;

        public LayoutListViewTab([NotNull] LayoutDesigner layoutDesigner)
        {
            InitializeComponent();

            LayoutDesigner = layoutDesigner;

            NoItems.DragOver += EmptyDragOver;
            NoItems.Drop += EmptyDrop;

            List.DragOver += HandleDragOver;
            List.Drop += HandleDrop;
            List.PreviewMouseLeftButtonDown += HandleMouseLeftButtonDown;
            List.PreviewMouseRightButtonDown += HandleMouseRightButtonDown;
            List.MouseMove += HandleMouseMove;

            List.ItemsSource = items;

            var view = (CollectionView)CollectionViewSource.GetDefaultView(List.ItemsSource);
            view.Filter = RenderingFilter;

            dragOrigin.X = double.MinValue;
            dragOrigin.Y = double.MinValue;

            listViewAdorner = new ListViewAdorner(List);

            Loaded += ControlLoaded;
        }

        public DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        public string DeviceId { get; set; }

        [NotNull]
        public string DeviceName { get; set; }

        public string Layout => LayoutSelector.Text;

        public LayoutDesigner LayoutDesigner { get; }

        [NotNull]
        public ItemUri LayoutUri { get; set; }

        public bool Modified
        {
            set
            {
                LayoutListView.RaiseModified();
                LayoutDesigner.UpdateRibbon(this);
            }
        }

        [NotNull]
        protected LayoutListView LayoutListView { get; set; }

        [CanBeNull]
        protected string LayoutName { get; set; }

        [CanBeNull]
        protected string LayoutPath { get; set; }

        [CanBeNull]
        protected IEnumerable<LayoutDesignerItem> SelectedItems { get; set; }

        private bool IsUpdatingColumns { get; set; }

        IEnumerable<RenderingItem> IRenderingContainer.Renderings => items.OfType<RenderingItem>();

        public void AddPlaceHolder([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var placeholderItem = new PlaceholderItem(databaseUri)
            {
                Id = @"content",
                Icon = new Icon("Resources/16x16/star_yellow.png"),
                UniqueId = Guid.NewGuid().ToString(@"B").ToUpperInvariant()
            };

            placeholderItem.Modified += SetModified;

            items.Add(placeholderItem);
            List.SelectedItem = placeholderItem;
            Modified = true;

            NoItems.Visibility = Visibility.Collapsed;
            ListContextMenu.Visibility = Visibility.Visible;
        }

        public void AddRendering([NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            var insertionIndex = List.SelectedIndex;
            var selectedRenderingIndex = List.SelectedIndex;
            if (insertionIndex < 0)
            {
                insertionIndex = items.Count;
                selectedRenderingIndex = insertionIndex;
            }
            else
            {
                insertionIndex++;
            }

            List.SelectedItem = AddRendering(rendering, insertionIndex, selectedRenderingIndex);

            NoItems.Visibility = Visibility.Collapsed;
            ListContextMenu.Visibility = Visibility.Visible;

            List.UpdateLayout();
            var selectedItem = List.ItemContainerGenerator.ContainerFromItem(List.SelectedItem) as ListViewItem;
            if (selectedItem != null)
            {
                selectedItem.Focus();
            }

            Modified = true;
        }

        public void CommitChanges()
        {
            foreach (var item in items)
            {
                item.Commit();
            }
        }

        [NotNull]
        public object GetContext()
        {
            var selectedItem = List.SelectedItem as LayoutDesignerItem;
            var selectedItems = List.SelectedItems.OfType<LayoutDesignerItem>();

            return new LayoutDesignerContext(LayoutListView.LayoutDesigner, selectedItem, selectedItems);
        }

        [CanBeNull]
        public IEnumerable<object> GetSelectedObjects()
        {
            SelectedItems = List.SelectedItems.OfType<LayoutDesignerItem>();

            return SelectedItems;
        }

        public void Initialize([NotNull] LayoutListView layoutListView, [NotNull] DatabaseUri databaseUri, [NotNull] string deviceId, [NotNull] string deviceName)
        {
            Assert.ArgumentNotNull(layoutListView, nameof(layoutListView));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(deviceId, nameof(deviceId));
            Assert.ArgumentNotNull(deviceName, nameof(deviceName));

            LayoutListView = layoutListView;
            LayoutUri = ItemUri.Empty;
            DatabaseUri = databaseUri;
            DeviceId = deviceId;
            DeviceName = deviceName;
        }

        public void LoadDevice([NotNull] XElement device)
        {
            Assert.ArgumentNotNull(device, nameof(device));

            items.Clear();

            DeviceId = device.GetAttributeValue("id");

            var layout = device.GetAttributeValue("l");
            if (!string.IsNullOrEmpty(layout))
            {
                LayoutUri = new ItemUri(DatabaseUri, new ItemId(new Guid(layout)));
            }
            else
            {
                LayoutUri = ItemUri.Empty;
            }

            LayoutName = device.GetAttributeValue("ln");
            LayoutPath = device.GetAttributeValue("lp");
            LayoutSelector.Text = device.GetAttributeValue("lp");

            foreach (var element in device.Elements(@"r"))
            {
                var renderingItem = new RenderingItem(this, DatabaseUri, element);
                renderingItem.Modified += SetModified;

                items.Add(renderingItem);
            }

            foreach (var element in device.Elements(@"p"))
            {
                var placeholderItem = new PlaceholderItem(DatabaseUri, element);

                placeholderItem.Modified += SetModified;

                items.Add(placeholderItem);
            }

            if (items.Count > 0 || !string.IsNullOrEmpty(LayoutSelector.Text))
            {
                ListContextMenu.Visibility = Visibility.Visible;
                NoItems.Visibility = Visibility.Collapsed;
                List.SelectedIndex = 0;
            }
            else
            {
                NoItems.Visibility = Visibility.Visible;
                ListContextMenu.Visibility = Visibility.Collapsed;
            }
        }

        public void OpenMenu([NotNull] object sender)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(sender, nameof(sender));

            var context = GetContext();

            var contextMenu = AppHost.ContextMenus.Build(context);
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = sender as UIElement;
            contextMenu.IsOpen = true;
        }

        public void Remove()
        {
            var selectedItems = new List<LayoutDesignerItem>();
            foreach (var selectedItem in List.SelectedItems.OfType<LayoutDesignerItem>())
            {
                selectedItems.Add(selectedItem);
            }

            if (selectedItems.Count == 0)
            {
                return;
            }

            var modified = false;
            var index = -1;

            foreach (var selectedItem in selectedItems)
            {
                var n = items.IndexOf(selectedItem);

                if (index < 0)
                {
                    index = n;
                }
                else if (index > n)
                {
                    index--;
                }

                items.Remove(selectedItem);
                modified = true;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= items.Count)
            {
                index = items.Count - 1;
            }

            if (index >= 0 && index < items.Count)
            {
                List.SelectedIndex = index;
            }

            if (modified)
            {
                Modified = true;
            }
        }

        public void RemoveRendering([NotNull] LayoutDesignerItem renderingItem)
        {
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            var index = items.IndexOf(renderingItem);

            items.Remove(renderingItem);

            if (index >= items.Count)
            {
                index = items.Count - 1;
            }

            if (index >= 0)
            {
                List.SelectedIndex = index;
            }

            Modified = true;
        }

        public void SaveLayout([NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            SaveLayout(output, false, string.Empty);
        }

        public void SaveLayout([NotNull] XmlTextWriter output, [NotNull] string deviceId)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(deviceId, nameof(deviceId));

            SaveLayout(output, true, deviceId);
        }

        public void UpdateTracking()
        {
            UpdateTracking(SelectedItems);
        }

        [NotNull]
        internal LayoutDesignerItem AddRendering([NotNull] RenderingItem rendering, int insertionIndex, int selectedRenderingIndex)
        {
            Debug.ArgumentNotNull(rendering, nameof(rendering));

            rendering.Modified += SetModified;

            if (insertionIndex == -1 || items.Count == 0)
            {
                items.Add(rendering);
            }
            else
            {
                var i = selectedRenderingIndex;
                if (i >= items.Count)
                {
                    i = items.Count - 1;
                }

                var targetRendering = items[i] as RenderingItem;
                if (targetRendering != null && !string.IsNullOrEmpty(targetRendering.PlaceholderKey.Key) && string.IsNullOrEmpty(rendering.PlaceholderKey.Key))
                {
                    rendering.PlaceholderKey = new PlaceHolderKey(targetRendering.PlaceholderKey.Key);
                }

                items.Insert(insertionIndex, rendering);
            }

            Action completed = delegate
            {
                var selectedItem = List.SelectedItem;
                if (selectedItem != null)
                {
                    List.SelectedItem = null;
                    List.SelectedItem = selectedItem;
                }

                List.Items.Refresh();
            };

            rendering.GetRenderingParameters(rendering.ItemUri, completed);

            return rendering;
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new LayoutBrowserDialog();
            d.Initialize(Rocks.Resources.Browse, DatabaseUri);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var selectedLayout = d.SelectedLayout;
            if (selectedLayout == null)
            {
                return;
            }

            LayoutUri = selectedLayout.LayoutUri;
            var path = d.SelectedPath;

            var n = path.IndexOf(@"/", StringComparison.Ordinal);
            if (n >= 0)
            {
                path = path.Mid(n);
            }

            LayoutSelector.Text = path;
            Modified = true;
        }

        private void Clear()
        {
            SelectedItems = null;
            AppHost.Selection.Track(LayoutListView.LayoutDesigner.Pane, null);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            List.Style = HiddenListView.Style;
            List.ItemStyle = HiddenListViewItem.Style;

            RefreshColumns();

            var gridView = List.View as GridView;
            if (gridView != null)
            {
                gridView.Columns.CollectionChanged += HandleReorderColumns;
            }
        }

        private void CopyRenderings([CanBeNull] LayoutDesignerItem target, ControlDragAdornerPosition position)
        {
            var selectedItems = List.SelectedItems.OfType<RenderingItem>().ToList();
            if (!selectedItems.Any())
            {
                return;
            }

            var index = target != null ? items.IndexOf(target) : items.Count;

            if (target != null && items.IndexOf(target) >= 0 && position == ControlDragAdornerPosition.Bottom)
            {
                index++;
            }

            if (index < 0)
            {
                index = 0;
            }

            string placeholderKey = null;

            var renderingTarget = target as RenderingItem;
            if (renderingTarget != null)
            {
                placeholderKey = renderingTarget.PlaceholderKey.Key;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("renderings");

            foreach (var selectedItem in selectedItems)
            {
                selectedItem.Write(output, true);
            }

            output.WriteEndElement();

            var root = writer.ToString().ToXElement();
            if (root == null)
            {
                return;
            }

            var renderings = root.Elements().Select(element => new RenderingItem(this, DatabaseUri, element, true)).ToList();

            foreach (var rendering in renderings)
            {
                items.Insert(index, rendering);

                if (!string.IsNullOrEmpty(placeholderKey))
                {
                    rendering.PlaceholderKey = new PlaceHolderKey(placeholderKey);
                }
            }

            List.SelectedItem = renderings.FirstOrDefault();
            Modified = true;
        }

        private void EditRendering([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (List.SelectedItems.Count != 1)
            {
                return;
            }

            var renderingItem = List.SelectedItem as RenderingItem;
            if (renderingItem == null)
            {
                return;
            }

            AppHost.Windows.OpenPropertyWindow();
            /*
            try
            {
                var dialog = new EditRenderingDialog(renderingItem);
                AppHost.Shell.ShowDialog(dialog);

                UpdateTracking(new[]
                {
                    renderingItem
                });
            }
            catch (Exception ex)
            {
                AppHost.Shell.HandleException(ex);
            }
            */
        }

        private void EmptyDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void EmptyDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                e.Handled = true;

                var itemList = (IEnumerable<IItem>)e.Data.GetData(DragManager.DragIdentifier);
                InsertRenderings(null, ControlDragAdornerPosition.Top, itemList);

                NoItems.Visibility = Visibility.Collapsed;
                ListContextMenu.Visibility = Visibility.Visible;
            }
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            CollectionViewSource.GetDefaultView(List.ItemsSource).Refresh();
        }

        void IRenderingContainer.GetDataBindingValues(RenderingItem renderingItem, DynamicProperty dynamicProperty, List<string> values)
        {
            Debug.ArgumentNotNull(renderingItem, nameof(renderingItem));
            Debug.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));
            Debug.ArgumentNotNull(values, nameof(values));

            this.GetRenderingContainerDataBindingValues(renderingItem, dynamicProperty, values);
        }

        [CanBeNull]
        private LayoutDesignerItem GetRendering([CanBeNull] DragEventArgs e)
        {
            if (e == null)
            {
                return null;
            }

            var source = e.OriginalSource as FrameworkElement;
            if (source == null)
            {
                return null;
            }

            var listViewItem = source.GetAncestorOrSelf<ListViewItem>();
            if (listViewItem == null)
            {
                return null;
            }

            return listViewItem.Content as LayoutDesignerItem;
        }

        private void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if (e.Data.GetDataPresent(LayoutDesigner.DragIdentifier))
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                }
            }

            if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void HandleDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var target = GetRendering(e);

            if (e.Data.GetDataPresent(LayoutDesigner.DragIdentifier))
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    CopyRenderings(target, ControlDragAdornerPosition.Top);
                }
                else
                {
                    MoveRenderings(target, ControlDragAdornerPosition.Top);
                }

                listViewAdorner.Drop(sender, e);
                e.Handled = true;
            }
            else if (e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                var itemList = (IEnumerable<IItem>)e.Data.GetData(DragManager.DragIdentifier);
                InsertRenderings(target, ControlDragAdornerPosition.Top, itemList);
                listViewAdorner.Drop(sender, e);
                e.Handled = true;
            }
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Delete)
            {
                Remove();
                e.Handled = true;
            }
        }

        private void HandleMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            DragManager.HandleMouseDown(this, e, out dragOrigin);
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var i = sender as RenderingItem;
            if (i != null)
            {
                e.Handled = true;
            }

            if (!DragManager.IsDragStart(this, e, ref dragOrigin))
            {
                return;
            }

            if (!UpdateSelection(e))
            {
                return;
            }

            var dragData = new DataObject(LayoutDesigner.DragIdentifier, this);

            DragManager.DoDragDrop(this, dragData, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);

            e.Handled = true;
        }

        private void HandleMouseRightButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;
        }

        private void HandleReorderColumns(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsUpdatingColumns)
            {
                return;
            }

            var gridView = List.View as GridView;
            if (gridView == null)
            {
                return;
            }

            var columns = string.Empty;

            foreach (var column in gridView.Columns)
            {
                var columnHeader = ((GridViewColumnHeader)column.Header).Content as string;

                columns = columns.Append(columnHeader, ',');
            }

            AppHost.Settings.SetString("LayoutDesigner", "Columns", columns);
        }

        private void HideColumn(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var columnHeader = menuItem.Tag as string;
            if (string.IsNullOrEmpty(columnHeader))
            {
                return;
            }

            var columns = AppHost.Settings.GetString("LayoutDesigner", "Columns", DefaultColumns).SplitEscaped(',').ToList().Where(c => c != columnHeader);

            var s = string.Empty;
            foreach (var c in columns)
            {
                s = s.Append(c, ',');
            }

            AppHost.Settings.SetString("LayoutDesigner", "Columns", s);

            RefreshColumns();
        }

        private void InsertRenderings([CanBeNull] LayoutDesignerItem target, ControlDragAdornerPosition position, [NotNull] IEnumerable<IItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            int index;
            if (target != null)
            {
                index = this.items.IndexOf(target);
                if (position == ControlDragAdornerPosition.Bottom)
                {
                    index++;
                }
            }
            else
            {
                index = this.items.Count;
            }

            string placeholderKey = null;
            var renderingTarget = target as RenderingItem;
            if (renderingTarget != null)
            {
                placeholderKey = renderingTarget.PlaceholderKey.Key;
            }

            var modified = false;
            LayoutDesignerItem selectedRendering = null;

            foreach (var item in items)
            {
                var renderingItem = new RenderingItem(this, item);

                var layoutItem = AddRendering(renderingItem, index, index);

                var rendering = layoutItem as RenderingItem;
                if (rendering != null && string.IsNullOrEmpty(rendering.PlaceholderKey.Key) && !string.IsNullOrEmpty(placeholderKey))
                {
                    rendering.PlaceholderKey = new PlaceHolderKey(placeholderKey);
                }

                if (selectedRendering == null)
                {
                    selectedRendering = layoutItem;
                }

                modified = true;
            }

            List.SelectedItem = selectedRendering;
            Keyboard.Focus(List);
            if (modified)
            {
                Modified = true;
            }
        }

        private void LayoutSelectorTextChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Modified = true;
            LayoutSelectorImage.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(LayoutSelector.Text))
            {
                LayoutUri = ItemUri.Empty;
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result, true))
                {
                    LayoutSelectorImage.Visibility = Visibility.Visible;
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    LayoutSelectorImage.Visibility = Visibility.Visible;
                }

                var itemHeader = ItemHeader.Parse(DatabaseUri, element);
                LayoutUri = itemHeader.ItemUri;
            };

            DatabaseUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, LayoutSelector.Text, DatabaseUri.DatabaseName.Name);
        }

        private void ListSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (List.SelectedItems.Count == 0)
            {
                Clear();
                return;
            }

            SelectedItems = List.SelectedItems.OfType<LayoutDesignerItem>();
            UpdateTracking(SelectedItems);
        }

        private void MoveRenderings([CanBeNull] LayoutDesignerItem target, ControlDragAdornerPosition position)
        {
            var selectedItems = new List<LayoutDesignerItem>();
            foreach (var selectedItem in List.SelectedItems.OfType<LayoutDesignerItem>())
            {
                selectedItems.Add(selectedItem);
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
            {
                var text = string.Format("Are you sure you want to move these {0} elements?", selectedItems.Count);

                if (selectedItems.Count == 1)
                {
                    var selectedItem = selectedItems.First();

                    var renderingItem = selectedItem as RenderingItem;
                    if (renderingItem != null)
                    {
                        text = string.Format("Are you sure you want to move the '{0}' rendering?", renderingItem.GetDisplayName());
                    }

                    var placeholderItem = selectedItem as PlaceholderItem;
                    if (placeholderItem != null)
                    {
                        text = string.Format("Are you sure you want to move the '{0}' placeholder?", placeholderItem.Name);
                    }
                }

                if (AppHost.MessageBox(text, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            var index = target != null ? items.IndexOf(target) : items.Count;

            foreach (var selectedItem in selectedItems)
            {
                var i = items.IndexOf(selectedItem);

                if (i < index)
                {
                    index--;
                }

                items.Remove(selectedItem);
            }

            if (target != null && items.IndexOf(target) >= 0 && position == ControlDragAdornerPosition.Bottom)
            {
                index++;
            }

            if (index < 0)
            {
                index = 0;
            }

            var modified = false;
            object selected = null;

            for (var n = selectedItems.Count - 1; n >= 0; n--)
            {
                var selectedItem = selectedItems[n];

                if (selected == null)
                {
                    selected = selectedItem;
                }

                items.Insert(index, selectedItem);
                modified = true;
            }

            List.SelectedItem = selected;

            if (modified)
            {
                Modified = true;
            }
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ListContextMenu.ContextMenu = null;

            if (!UpdateSelection(e))
            {
                return;
            }

            var context = GetContext();

            var commands = Rocks.Commands.CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ListContextMenu.ContextMenu = contextMenu;
        }

        private void OpenNoItemsContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            var context = GetContext();

            NoItemsPanel.ContextMenu = ContextMenuExtensions.GetContextMenu(context, e);
        }

        private void RefreshColumns()
        {
            var gridView = List.View as GridView;
            if (gridView == null)
            {
                return;
            }

            IsUpdatingColumns = true;

            gridView.Columns.Clear();

            var columns = AppHost.Settings.GetString("LayoutDesigner", "Columns", DefaultColumns).Split(',');
            foreach (var columnHeader in columns)
            {
                var columnValue = columnHeader;
                switch (columnHeader)
                {
                    case "ID":
                        columnValue = "Id";
                        break;
                    case "Placeholder":
                        columnValue = "PlaceholderKey";
                        break;
                    case "Data Source":
                        columnValue = "DataSource";
                        break;
                }

                var gridViewColumn = new GridViewColumn();
                var gridViewColumnHeader = new GridViewColumnHeader();

                gridViewColumn.Header = gridViewColumnHeader;

                var contextMenu = new ContextMenu();

                var menuItem = new MenuItem
                {
                    Header = "Size Column to Fit",
                    Tag = columnHeader
                };
                menuItem.Click += SizeColumnToFit;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem
                {
                    Header = "Size All Columns to Fit"
                };
                menuItem.Click += SizeAllColumnsToFit;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                menuItem = new MenuItem
                {
                    Header = "Select Columns..."
                };
                menuItem.Click += SelectColumns;
                contextMenu.Items.Add(menuItem);

                if (columnHeader != "Rendering")
                {
                    menuItem = new MenuItem
                    {
                        Header = "Hide Column",
                        Tag = columnHeader
                    };
                    menuItem.Click += HideColumn;
                    contextMenu.Items.Add(menuItem);
                }

                gridViewColumnHeader.ContextMenu = contextMenu;

                switch (columnHeader)
                {
                    case "Rendering":
                        gridViewColumnHeader.Content = "Rendering";
                        gridViewColumn.CellTemplate = FindResource("RenderingHeader") as DataTemplate;
                        break;

                    default:
                        gridViewColumnHeader.Content = columnHeader;
                        gridViewColumn.DisplayMemberBinding = new Binding(columnValue);
                        break;
                }

                gridView.Columns.Add(gridViewColumn);
            }

            IsUpdatingColumns = false;
        }

        private bool RenderingFilter([NotNull] object obj)
        {
            Debug.ArgumentNotNull(obj, nameof(obj));

            var placeholder = obj as PlaceholderItem;
            if (placeholder != null)
            {
                return placeholder.Name.IsFilterMatch(Filter.Text);
            }

            var renderingItem = obj as RenderingItem;
            if (renderingItem != null)
            {
                return renderingItem.Name.IsFilterMatch(Filter.Text);
            }

            return true;
        }

        private void SaveLayout([NotNull] XmlTextWriter output, bool isCopy, [NotNull] string deviceId)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(deviceId, nameof(deviceId));

            if (string.IsNullOrEmpty(DeviceId))
            {
                return;
            }

            if (items.Count == 0 && LayoutUri == ItemUri.Empty)
            {
                return;
            }

            output.WriteStartElement(@"d");
            output.WriteAttributeStringNotEmpty(@"id", isCopy ? deviceId : DeviceId);

            if (LayoutUri != ItemUri.Empty)
            {
                output.WriteAttributeStringNotEmpty(@"l", LayoutUri.ItemId.ToString());
            }
            else
            {
                output.WriteAttributeStringNotEmpty(@"l", string.Empty);
            }

            if (isCopy)
            {
                output.WriteAttributeStringNotEmpty(@"ln", LayoutName);
                output.WriteAttributeStringNotEmpty(@"lp", LayoutPath);
            }

            foreach (var item in items)
            {
                var listItem = item;

                if (listItem != null)
                {
                    listItem.Write(output, isCopy);
                }
            }

            output.WriteEndElement();
        }

        private void SelectColumns(object sender, RoutedEventArgs e)
        {
            var columns = AppHost.Settings.GetString("LayoutDesigner", "Columns", DefaultColumns).SplitEscaped(',').ToList();

            var dialog = new SelectColumnsDialog(((IRenderingContainer)this).Renderings, columns);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var s = string.Empty;
            foreach (var columnHeader in dialog.SelectColumns)
            {
                s = s.Append(columnHeader, ',');
            }

            AppHost.Settings.SetString("LayoutDesigner", "Columns", s);

            RefreshColumns();
        }

        private void SetModified([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Modified = true;
        }

        private void SizeAllColumnsToFit(object sender, RoutedEventArgs e)
        {
            var gridView = List.View as GridView;
            if (gridView == null)
            {
                return;
            }

            foreach (var column in gridView.Columns)
            {
                List.ResizeColumn(column);
            }
        }

        private void SizeColumnToFit(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var columnHeader = menuItem.Tag as string;
            if (string.IsNullOrEmpty(columnHeader))
            {
                return;
            }

            var gridView = List.View as GridView;
            if (gridView == null)
            {
                return;
            }

            var column = gridView.Columns.FirstOrDefault(c => ((GridViewColumnHeader)c.Header).Content as string == columnHeader);
            if (column == null)
            {
                return;
            }

            List.ResizeColumn(column);
        }

        private bool UpdateSelection([NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var source = e.OriginalSource as FrameworkElement;
            if (source == null)
            {
                return false;
            }

            var listViewItem = source.GetAncestorOrSelf<ListViewItem>();
            if (listViewItem == null)
            {
                return false;
            }

            var rendering = listViewItem.Content as LayoutDesignerItem;
            if (List.SelectedItems.Contains(rendering))
            {
                return true;
            }

            List.SelectedItem = rendering;

            return true;
        }

        private void UpdateTracking([CanBeNull] IEnumerable<object> selectedItems)
        {
            LayoutDesigner.UpdateRibbon(this);
            AppHost.Selection.Track(LayoutListView.LayoutDesigner.Pane, selectedItems);
        }
    }
}
