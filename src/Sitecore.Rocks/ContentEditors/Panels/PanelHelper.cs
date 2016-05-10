// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Skins;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Panels
{
    public class PanelHelper
    {
        private readonly List<TabItemInfo> tabs = new List<TabItemInfo>();

        public PanelHelper([NotNull] TabControl tabs, [NotNull] DockPanel outerDock, [NotNull] DockPanel innerDock, [NotNull] Grid tabPanels)
        {
            Assert.ArgumentNotNull(tabs, nameof(tabs));
            Assert.ArgumentNotNull(outerDock, nameof(outerDock));
            Assert.ArgumentNotNull(innerDock, nameof(innerDock));
            Assert.ArgumentNotNull(tabPanels, nameof(tabPanels));

            Tabs = tabs;
            OuterDock = outerDock;
            InnerDock = innerDock;
            TabPanels = tabPanels;
        }

        public DockPanel InnerDock { get; set; }

        public DockPanel OuterDock { get; set; }

        public Grid TabPanels { get; set; }

        public TabControl Tabs { get; set; }

        public void DockFill([NotNull] string tabHeader, double priority, [NotNull] Control userControl)
        {
            Assert.ArgumentNotNull(tabHeader, nameof(tabHeader));
            Assert.ArgumentNotNull(userControl, nameof(userControl));

            var tabItemInfo = new TabItemInfo
            {
                Header = tabHeader,
                Priority = priority,
                UserControl = userControl
            };

            tabs.Add(tabItemInfo);
            TabPanels.Children.Add(userControl);
        }

        public void DockInner([NotNull] Control userControl, Dock dockPosition)
        {
            Assert.ArgumentNotNull(userControl, nameof(userControl));

            var count = InnerDock.Children.Count;
            if (count == 0)
            {
                InnerDock.Children.Add(userControl);
            }
            else
            {
                InnerDock.Children.Insert(count - 1, userControl);
            }

            userControl.SetValue(DockPanel.DockProperty, dockPosition);
        }

        public void DockOuter([NotNull] Control userControl, Dock dockPosition)
        {
            Assert.ArgumentNotNull(userControl, nameof(userControl));

            var count = OuterDock.Children.Count;
            if (count == 0)
            {
                OuterDock.Children.Add(userControl);
            }
            else
            {
                for (var n = count - 1; n >= 0; n--)
                {
                    var frameworkElement = OuterDock.Children[n];

                    if (frameworkElement is TabControl)
                    {
                        OuterDock.Children.Insert(n, userControl);
                        break;
                    }
                }
            }

            userControl.SetValue(DockPanel.DockProperty, dockPosition);
        }

        public void RenderPanels([NotNull] ISkin skin, [NotNull] ContentModel contentModel, [NotNull] IEnumerable<IPanel> panels)
        {
            Assert.ArgumentNotNull(skin, nameof(skin));
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));
            Assert.ArgumentNotNull(panels, nameof(panels));

            var customizationContext = new PanelContext(skin, contentModel);

            tabs.Clear();
            Tabs.Items.Clear();

            var contentTab = new TabItemInfo
            {
                Header = Resources.DefaultSkin_InitializeCustomizations_Content,
                UserControl = InnerDock,
                Priority = 1000
            };

            tabs.Add(contentTab);

            foreach (var panel in panels.Reverse())
            {
                panel.Render(customizationContext);
            }

            if (tabs.Count == 1)
            {
                Tabs.Visibility = Visibility.Collapsed;
                return;
            }

            tabs.Sort((tab0, tab1) => tab0.Priority.CompareTo(tab1.Priority));

            foreach (var tabItemInfo in tabs)
            {
                var tabItem = new TabItem
                {
                    Header = tabItemInfo.Header,
                    Tag = tabItemInfo,
                    IsSelected = tabItemInfo.Priority == 1000
                };

                tabItemInfo.UserControl.Visibility = tabItem.IsSelected ? Visibility.Visible : Visibility.Collapsed;

                Tabs.Items.Add(tabItem);
            }
        }

        public void TabSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs selectionChangedEventArgs)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(selectionChangedEventArgs, nameof(selectionChangedEventArgs));

            var tabItem = Tabs.SelectedItem as TabItem;
            if (tabItem == null)
            {
                return;
            }

            foreach (var tabItemInfo in tabs)
            {
                tabItemInfo.UserControl.Visibility = tabItem.Tag == tabItemInfo ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private class TabItemInfo
        {
            public string Header { get; set; }

            public double Priority { get; set; }

            public FrameworkElement UserControl { get; set; }
        }
    }
}
