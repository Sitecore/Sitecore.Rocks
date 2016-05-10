// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.Links
{
    public partial class LinkViewer
    {
        public LinkViewer()
        {
            InitializeComponent();

            EnableTabs();
        }

        public IEditorPane Pane { get; set; }

        public void CloseTab([NotNull] TabItem tabItem)
        {
            Assert.ArgumentNotNull(tabItem, nameof(tabItem));

            Tabs.Items.Remove(tabItem);

            if (Tabs.Items.Count == 0)
            {
                Pane.Close();
            }

            EnableTabs();
        }

        [NotNull]
        public LinkTab CreateTab([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            foreach (var i in Tabs.Items)
            {
                var t = i as TabItem;
                if (t == null)
                {
                    continue;
                }

                var l = t.Content as LinkTab;
                if (l == null)
                {
                    continue;
                }

                if (l.ItemUri != itemUri)
                {
                    continue;
                }

                t.IsSelected = true;
                return l;
            }

            var tabItem = new TabItem();

            var tab = new LinkTab
            {
                ItemUri = itemUri,
                LinkViewer = this,
                TabItem = tabItem
            };

            var tabHeader = new TabItemHeader
            {
                Header = Rocks.Resources.LinkViewer_CreateTab_Loading___,
                Tag = tab
            };

            tabHeader.Click += delegate { CloseTab(tabItem); };
            tabItem.Header = tabHeader;

            Tabs.Items.Add(tabItem);

            tabItem.Content = tab;

            tabItem.IsSelected = true;

            EnableTabs();

            return tab;
        }

        private void EnableTabs()
        {
            if (Tabs.Items.Count > 0)
            {
                NoTabs.Visibility = Visibility.Collapsed;
                Tabs.Visibility = Visibility.Visible;
            }
            else
            {
                NoTabs.Visibility = Visibility.Visible;
                Tabs.Visibility = Visibility.Collapsed;
            }
        }
    }
}
