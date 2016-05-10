// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews
{
    public partial class LayoutListView : ILayoutDesignerView
    {
        public LayoutListView([NotNull] LayoutDesigner layoutDesigner)
        {
            Assert.ArgumentNotNull(layoutDesigner, nameof(layoutDesigner));

            InitializeComponent();

            LayoutDesigner = layoutDesigner;
            Tabs = new List<LayoutListViewTab>();
        }

        [NotNull]
        public LayoutDesigner LayoutDesigner { get; }

        [NotNull]
        public List<LayoutListViewTab> Tabs { get; set; }

        public void Activate([NotNull] string deviceName)
        {
            Assert.ArgumentNotNull(deviceName, nameof(deviceName));

            foreach (var item in Devices.Items)
            {
                var tab = item as TabItem;
                if (tab == null)
                {
                    continue;
                }

                if (tab.Header as string == deviceName)
                {
                    tab.IsSelected = true;
                    return;
                }
            }
        }

        public void AddPlaceholder(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            var selectedTab = Devices.SelectedItem as TabItem;
            if (selectedTab == null)
            {
                return;
            }

            var tab = selectedTab.Content as LayoutListViewTab;
            if (tab == null)
            {
                return;
            }

            tab.AddPlaceHolder(databaseUri);
        }

        public void AddRendering(RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            var selectedTab = Devices.SelectedItem as TabItem;
            if (selectedTab == null)
            {
                return;
            }

            var tab = selectedTab.Content as LayoutListViewTab;
            if (tab == null)
            {
                return;
            }

            tab.AddRendering(rendering);
        }

        [CanBeNull]
        public LayoutListViewTab GetActiveListView()
        {
            var tab = Devices.SelectedItem as TabItem;
            if (tab == null)
            {
                return null;
            }

            return tab.Content as LayoutListViewTab;
        }

        public object GetContext()
        {
            var listView = GetActiveListView();
            if (listView == null)
            {
                return null;
            }

            return listView.GetContext();
        }

        public IEnumerable<object> GetSelectedObjects()
        {
            var selectedTab = Devices.SelectedItem as TabItem;
            if (selectedTab == null)
            {
                return null;
            }

            var tab = selectedTab.Content as LayoutListViewTab;
            if (tab == null)
            {
                return null;
            }

            return tab.GetSelectedObjects();
        }

        public void LoadLayout(DatabaseUri databaseUri, XElement layoutDefinition)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(layoutDefinition, nameof(layoutDefinition));

            var devices = layoutDefinition.Element(@"devices");
            if (devices == null)
            {
                return;
            }

            Devices.Items.Clear();
            Tabs.Clear();

            var layoutElement = layoutDefinition.Element(@"layout");

            foreach (var device in devices.Elements(@"d"))
            {
                var tabItem = new TabItem();
                Devices.Items.Add(tabItem);

                // tabItem.Height = 32;
                tabItem.Header = device.Value;

                var tab = new LayoutListViewTab(LayoutDesigner);
                Tabs.Add(tab);
                tab.Initialize(this, databaseUri, device.GetAttributeValue("id"), device.Value);

                if (layoutElement != null)
                {
                    var deviceId = device.GetAttributeValue("id");

                    var deviceElement = layoutElement.Elements(@"d").FirstOrDefault(element => element.GetAttributeValue("id") == deviceId);
                    if (deviceElement != null)
                    {
                        tab.LoadDevice(deviceElement);
                    }
                }

                tabItem.Content = tab;
            }
        }

        public event EventHandler Modified;

        public void OpenMenu(object sender)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));

            var tab = GetActiveListView();
            if (tab != null)
            {
                tab.OpenMenu(sender);
            }
        }

        public void RaiseModified()
        {
            var modified = Modified;
            if (modified != null)
            {
                modified(this, EventArgs.Empty);
            }
        }

        public void RemoveRendering(LayoutDesignerItem renderingItem)
        {
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            var tab = GetActiveListView();
            if (tab != null)
            {
                tab.RemoveRendering(renderingItem);
            }
        }

        public void SaveLayout(XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            foreach (var tab in Tabs)
            {
                tab.CommitChanges();
                tab.SaveLayout(output);
            }
        }

        public void UpdateTracking()
        {
            var tab = GetActiveListView();
            if (tab != null)
            {
                tab.UpdateTracking();
            }
        }

        IRenderingContainer ILayoutDesignerView.GetRenderingContainer()
        {
            return GetActiveListView();
        }
    }
}
