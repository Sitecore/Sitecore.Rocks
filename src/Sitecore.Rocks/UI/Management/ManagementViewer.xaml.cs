// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.Management
{
    public partial class ManagementViewer : IPane
    {
        public ManagementViewer()
        {
            InitializeComponent();
        }

        [NotNull]
        public string Caption
        {
            get { return Pane.Caption ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Pane.Caption = value;
            }
        }

        [NotNull]
        public IManagementContext Context { get; private set; }

        [NotNull]
        public IPane Pane { get; set; }

        public void Initialize([NotNull] string caption, [NotNull] IManagementContext context, [NotNull] string defaultItemName)
        {
            Assert.ArgumentNotNull(caption, nameof(caption));
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(defaultItemName, nameof(defaultItemName));

            if (Tabs.Items.Count > 0)
            {
                SelectItem(defaultItemName);
                return;
            }

            Context = context;

            LoadItems(defaultItemName);

            Caption = caption;
        }

        public void SelectItem([NotNull] string itemName)
        {
            Assert.ArgumentNotNull(itemName, nameof(itemName));

            foreach (var tabItem in Tabs.Items.OfType<TabItem>())
            {
                var managementItem = tabItem.Tag as IManagementItem;
                if (managementItem == null)
                {
                    continue;
                }

                var header = tabItem.Header as string ?? @"#";
                if (header == itemName)
                {
                    tabItem.IsSelected = true;
                    break;
                }
            }
        }

        private void LoadItems([NotNull] string defaultItemName)
        {
            Debug.ArgumentNotNull(defaultItemName, nameof(defaultItemName));

            Tabs.Items.Clear();

            var hasSelectedPage = false;

            foreach (var descriptor in ManagementManager.Items.OrderBy(i => i.Priority).ThenBy(i => i.Header))
            {
                var item = descriptor.GetManagementItem();
                if (item == null)
                {
                    continue;
                }

                if (!item.CanExecute(Context))
                {
                    continue;
                }

                var tabItem = new TabItem
                {
                    Header = descriptor.Header,
                    Tag = item,

                    // Height = 32
                };

                Tabs.Items.Add(tabItem);

                var header = tabItem.Header as string ?? @"#";
                if (header == defaultItemName)
                {
                    tabItem.IsSelected = true;
                    hasSelectedPage = true;
                }
            }

            if (!hasSelectedPage && Tabs.Items.Count > 0)
            {
                ((TabItem)Tabs.Items[0]).IsSelected = true;
            }
        }

        private void TabChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var tabItem = Tabs.SelectedItem as TabItem;
            if (tabItem == null)
            {
                Trace.Expected(typeof(TabItem));
                return;
            }

            if (tabItem.Content != null)
            {
                return;
            }

            var item = tabItem.Tag as IManagementItem;
            if (item == null)
            {
                return;
            }

            tabItem.Content = item.GetControl(Context);
        }
    }
}
