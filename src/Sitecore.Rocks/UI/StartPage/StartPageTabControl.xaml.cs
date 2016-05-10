// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage
{
    public enum TabStyle
    {
        Tab,

        Page,
    }

    public partial class StartPageTabControl
    {
        public StartPageTabControl([NotNull] StartPageViewer startPage)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));

            InitializeComponent();

            StartPage = startPage;
            ParentName = string.Empty;

            TabStyle = TabStyle.Tab;

            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public string ParentName { get; set; }

        public TabStyle TabStyle { get; set; }

        [NotNull]
        protected StartPageViewer StartPage { get; set; }

        public void RenderStartPage()
        {
            ApplyTabStyle();
            RenderTabs();
        }

        private void ApplyTabItemStyle([NotNull] TabItem tabItem)
        {
            Debug.ArgumentNotNull(tabItem, nameof(tabItem));

            switch (TabStyle)
            {
                case TabStyle.Tab:
                    tabItem.Style = (Style)FindResource(@"MainTabItem");
                    break;
                case TabStyle.Page:
                    tabItem.Style = (Style)FindResource(@"CategoryTabItem");
                    break;
            }
        }

        private void ApplyTabStyle()
        {
            switch (TabStyle)
            {
                case TabStyle.Tab:
                    Tabs.Style = (Style)FindResource(@"MainTabControl");
                    break;
                case TabStyle.Page:
                    Tabs.Style = (Style)FindResource(@"CategoryTabControl");
                    break;
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            RenderStartPage();
        }

        private void RenderTabs()
        {
            Tabs.Items.Clear();

            var descriptors = StartPageManager.Controls.Where(d => d.Attribute.ParentName == ParentName).ToList();

            foreach (var descriptor in descriptors.OrderBy(t => t.Attribute.Priority))
            {
                var instance = descriptor.GetInstance(StartPage);
                if (instance == null)
                {
                    continue;
                }

                var control = instance.GetControl(instance.ParentName);
                if (control == null)
                {
                    continue;
                }

                var tabItem = new TabItem
                {
                    Header = instance.Text,
                    Content = control
                };

                ApplyTabItemStyle(tabItem);

                Tabs.Items.Add(tabItem);

                if (Tabs.Items.Count == 1)
                {
                    tabItem.IsSelected = true;
                }
            }
        }
    }
}
