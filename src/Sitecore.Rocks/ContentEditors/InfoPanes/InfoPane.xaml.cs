// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.InfoPanes
{
    public partial class InfoPane
    {
        private readonly List<IInfoPane> infoPanes = new List<IInfoPane>();

        private ContentEditor contentEditor;

        public InfoPane()
        {
            InitializeComponent();

            var options = AppHost.Settings.Options;
            if (options.HideQuickInfo)
            {
                Visibility = Visibility.Collapsed;
            }

            Loaded += ControlLoaded;
        }

        [NotNull]
        public ContentEditor ContentEditor
        {
            get { return contentEditor; }
        }

        [NotNull]
        public ContentModel ContentModel
        {
            get { return ContentEditor.ContentModel; }
        }

        protected bool IsMouseDown { get; private set; }

        protected Point Position { get; set; }

        protected double TargetHeight { get; set; }

        private bool IsLoading { get; set; }

        public void Load([NotNull] ContentEditor editor)
        {
            Assert.ArgumentNotNull(editor, nameof(editor));

            contentEditor = editor;

            RenderTitle();
            RenderPanes();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            double height = 87;

            var value = AppHost.Settings.Get("ContentEditor\\InfoPane", "Height", height.ToString()) as string ?? string.Empty;

            double h;
            if (double.TryParse(value, out h))
            {
                height = h;
            }

            Height = height;
        }

        private void HandleMouseDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsMouseDown = true;
            Position = PointToScreen(e.GetPosition(this));
            TargetHeight = ActualHeight;

            Mouse.Capture(Resizer, CaptureMode.Element);
            e.Handled = true;
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!IsMouseDown)
            {
                return;
            }

            var position = PointToScreen(e.GetPosition(this));

            var dy = position.Y - Position.Y;
            var height = TargetHeight + dy;

            if (height < 35 || height > 800)
            {
                return;
            }

            Height = height;
            e.Handled = true;
        }

        private void HandleMouseUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsMouseDown = false;
            Mouse.Capture(null);
            e.Handled = true;

            AppHost.Settings.Set("ContentEditor\\InfoPane", "Height", ActualHeight.ToString());
        }

        private void RenderPanes()
        {
            IsLoading = true;

            var selectedItem = AppHost.Settings.GetString("ContentEditor", "InfoPane", string.Empty);

            Tabs.Items.Clear();

            TabItem selectedTabItem = null;

            foreach (var descriptor in InfoPaneManager.Panes.OrderBy(p => p.Priority).ThenBy(p => p.Header))
            {
                var instance = descriptor.GetInstance();

                if (!instance.CanRender(ContentEditor))
                {
                    continue;
                }

                infoPanes.Add(instance);

                var tabItem = new TabItem
                {
                    Tag = descriptor,
                    Height = 24
                };

                Tabs.Items.Add(tabItem);

                tabItem.Header = instance.GetHeader();
                tabItem.Content = instance.Render(ContentEditor);

                if (descriptor.Header == selectedItem)
                {
                    selectedTabItem = tabItem;
                }
            }

            IsLoading = false;

            if (selectedTabItem != null)
            {
                selectedTabItem.IsSelected = true;
            }
        }

        private void RenderTitle()
        {
            if (ContentModel.IsEmpty)
            {
                Title.Text = string.Empty;
                return;
            }

            if (ContentModel.IsMultiple)
            {
                Title.Text = string.Format("{0} items", ContentModel.Items.Count);
                return;
            }

            var item = ContentModel.Items.First();
            Title.Text = item.Name;
        }

        private void SaveTab([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (IsLoading)
            {
                return;
            }

            var tabControl = sender as TabControl;
            if (tabControl == null)
            {
                return;
            }

            var tabItem = tabControl.SelectedItem as TabItem;
            if (tabItem == null)
            {
                return;
            }

            var descriptor = tabItem.Tag as InfoPaneManager.InfoPaneDescriptor;
            if (descriptor == null)
            {
                return;
            }

            AppHost.Settings.Set("ContentEditor", "InfoPane", descriptor.Header);

            var tab = tabItem.Content as ICanActivateInfoPane;
            if (tab != null)
            {
                tab.Activate();
            }
        }
    }
}
