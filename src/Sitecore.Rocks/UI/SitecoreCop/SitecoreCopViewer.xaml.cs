// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.SitecoreCop
{
    public partial class SitecoreCopViewer : IHasEditorPane
    {
        public SitecoreCopViewer()
        {
            InitializeComponent();

            EnableTabs();
        }

        public IEditorPane Pane { get; set; }

        public void CloseTab([NotNull] TabItem tabItem)
        {
            Assert.ArgumentNotNull(tabItem, nameof(tabItem));

            Tabs.Items.Remove(tabItem);

            if (Tabs.Items.Count == 0 && Pane != null)
            {
                Pane.Close();
            }

            EnableTabs();
        }

        public void CreateTab([NotNull] ItemUri itemUri, bool deep)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            foreach (var i in Tabs.Items)
            {
                var t = i as TabItem;
                if (t == null)
                {
                    continue;
                }

                var l = t.Content as SitecoreCopTab;
                if (l == null)
                {
                    continue;
                }

                if (l.ItemUri != itemUri || l.Deep != deep)
                {
                    continue;
                }

                t.IsSelected = true;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var item = ItemHeader.Parse(itemUri.DatabaseUri, element);

                var tabItem = new TabItem();

                var tab = new SitecoreCopTab
                {
                    ItemUri = itemUri,
                    TabItem = tabItem
                };

                var tabHeader = new TabItemHeader
                {
                    Header = item.Name + (deep ? " (Deep)" : string.Empty),
                    Tag = tab
                };

                tabHeader.Click += delegate { CloseTab(tabItem); };
                tabItem.Header = tabHeader;

                Tabs.Items.Add(tabItem);

                tabItem.Content = tab;
                tabItem.IsSelected = true;

                EnableTabs();

                tab.Initialize(itemUri, deep);
            };

            itemUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, itemUri.ItemId.ToString(), itemUri.DatabaseName.Name);
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
