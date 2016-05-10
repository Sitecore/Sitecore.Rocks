// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Chunks;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Panes
{
    [Export(typeof(ISelectRenderingsDialogPane), Priority = 2000)]
    public partial class RenderingChunksPane : ISelectRenderingsDialogPane
    {
        public const string RegistryKey = "RenderingChunksPane";

        private readonly ListViewSorter listViewSorter;

        private readonly List<ItemHeader> renderings = new List<ItemHeader>();

        [CanBeNull]
        private CollectionView view;

        public RenderingChunksPane()
        {
            InitializeComponent();

            AllowMultipleRenderings = true;
            Header = "Chunks";

            FilterTextBox.Text = AppHost.Settings.GetString(RegistryKey, "Filter", string.Empty);
            listViewSorter = new ListViewSorter(ChunksListView);

            AppHost.Extensibility.ComposeParts(this);

            Loaded += ControlLoaded;
        }

        public bool AllowMultipleRenderings { get; }

        public DatabaseUri DatabaseUri { get; set; }

        public string Header { get; }

        public IRenderingContainer RenderingContainer { get; set; }

        public string SpeakCoreVersion { get; set; } = string.Empty;

        public bool AreButtonsEnabled()
        {
            return ChunksListView.SelectedItem != null;
        }

        public void Close()
        {
        }

        public event MouseButtonEventHandler DoubleClick;

        public void GetSelectedRenderings([NotNull] Action<IEnumerable<RenderingItem>> completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            var selectedItem = ChunksListView.SelectedItem as IRenderingChunk;
            if (selectedItem == null)
            {
                completed(Enumerable.Empty<RenderingItem>());
                return;
            }

            selectedItem.GetRenderings(RenderingContainer, completed);
        }

        public event SelectionChangedEventHandler SelectionChanged;

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadChunks();

            Keyboard.Focus(FilterTextBox.TextBox);
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (view != null)
            {
                view.Refresh();
            }

            AppHost.Settings.SetString(RegistryKey, "Filter", FilterTextBox.Text);
        }

        [CanBeNull]
        private string GetGroupName([NotNull] object sender)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));

            var expander = sender as Expander;
            if (expander == null)
            {
                return null;
            }

            var group = expander.Tag as CollectionViewGroup;
            if (group == null)
            {
                return null;
            }

            return group.Name as string ?? string.Empty;
        }

        private void HandleMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                if (frameworkElement.GetAncestorOrSelf<RepeatButton>() != null)
                {
                    return;
                }

                if (frameworkElement.GetAncestorOrSelf<GridViewColumnHeader>() != null)
                {
                    return;
                }

                if (frameworkElement.GetAncestorOrSelf<Thumb>() != null)
                {
                    return;
                }
            }

            var doubleClick = DoubleClick;
            if (doubleClick != null)
            {
                doubleClick(sender, e);
            }
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void InitExpander([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var expander = sender as Expander;
            if (expander == null)
            {
                return;
            }

            var name = GetGroupName(sender);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (!Storage.ReadBool(RegistryKey + "\\Groups", name, true))
            {
                expander.IsExpanded = false;
            }
        }

        private void LoadChunks()
        {
            var renderingChunkManager = AppHost.Container.Get<RenderingChunkManager>();

            renderingChunkManager.Refresh();

            var items = renderingChunkManager.RenderingChunks.ToList();

            ChunksListView.ItemsSource = items;

            listViewSorter.Resort();
            view = CollectionViewSource.GetDefaultView(items) as CollectionView;
            if (view == null)
            {
                return;
            }

            var groupDescription = new PropertyGroupDescription("Group")
            {
                StringComparison = StringComparison.InvariantCultureIgnoreCase
            };

            var collection = view.GroupDescriptions;
            if (collection != null)
            {
                collection.Clear();
                collection.Add(groupDescription);
            }

            view.Filter = delegate(object o)
            {
                var itemHeader = o as IRenderingChunk;
                if (itemHeader == null)
                {
                    return true;
                }

                return itemHeader.Name.IsFilterMatch(FilterTextBox.Text);
            };

            view.Refresh();

            ChunksListView.ResizeColumn(NameColumn);
        }

        private void RenderingsSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(sender, e);
            }
        }

        private void SetCollapsed([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var name = GetGroupName(sender);
            if (!string.IsNullOrEmpty(name))
            {
                Storage.WriteBool(RegistryKey + "\\Groups", name, false);
            }
        }

        private void SetExpanded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var name = GetGroupName(sender);
            if (!string.IsNullOrEmpty(name))
            {
                Storage.WriteBool(RegistryKey + "\\Groups", name, true);
            }
        }
    }
}
