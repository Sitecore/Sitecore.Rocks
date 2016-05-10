// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.Sites
{
    public partial class SiteEditor
    {
        public SiteEditor()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        [CanBeNull]
        public ISiteEditorControl DataServiceEditor { get; set; }

        [NotNull]
        public Site Site { get; set; }

        public void EnableButtons()
        {
            var dataServiceEditor = DataServiceEditor;
            if (dataServiceEditor != null)
            {
                dataServiceEditor.EnableButtons();
            }
        }

        public void Load([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;
            Update(site);

            LoadConnections();

            EnableButtons();
        }

        private void ConnectionsChanged([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(routedEventArgs, nameof(routedEventArgs));

            ConnectionButton.IsOpen = false;

            var item = sender as MenuItem;
            if (item == null)
            {
                return;
            }

            var site = item.Tag as Site;
            if (site == null)
            {
                return;
            }

            var index = 0;

            foreach (var n in DataDriver.Items)
            {
                var name = n as string ?? string.Empty;

                if (name == site.DataServiceName)
                {
                    DataDriver.SelectedIndex = index;
                    break;
                }

                index++;
            }

            var dataServiceEditor = DataServiceEditor;
            if (dataServiceEditor != null)
            {
                dataServiceEditor.Display(site);
            }

            EnableButtons();
        }

        private void DataProviderChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var oldControl = Editor.Child as ISiteEditorControl;

            Editor.Child = null;
            DataServiceEditor = null;

            var dataServiceName = DataDriver.SelectedValue as string ?? string.Empty;

            var control = DataServiceManager.GetSiteEditorControl(dataServiceName);
            if (control != null)
            {
                DataServiceEditor = control as ISiteEditorControl;
                Assert.IsNotNull(DataServiceEditor, "The Site Editor Control must implement the ISiteEditorControl interface");

                DataServiceEditor.SiteEditor = this;
                Editor.Child = control;

                if (oldControl != null)
                {
                    DataServiceEditor.CopyFrom(oldControl);
                }
            }

            EnableButtons();
        }

        private void LoadConnections()
        {
            var contextMenu = new StackPanel();

            foreach (var site in SiteManager.Sites)
            {
                if (site.GetTreeViewItem() == null)
                {
                    continue;
                }

                var item = new MenuItem
                {
                    Header = site.Name,
                    Tag = site
                };

                item.Click += ConnectionsChanged;

                contextMenu.Children.Add(item);
            }

            if (contextMenu.Children.Count == 0)
            {
                ConnectionButton.IsEnabled = false;
            }

            ConnectionButton.DropDownContent = contextMenu;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dataServiceEditor = DataServiceEditor;
            if (dataServiceEditor == null)
            {
                return;
            }

            if (!dataServiceEditor.Validate())
            {
                return;
            }

            dataServiceEditor.Apply(Site);

            this.Close(true);
        }

        private void TestClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dataServiceEditor = DataServiceEditor;
            if (dataServiceEditor == null)
            {
                return;
            }

            dataServiceEditor.Test();
        }

        private void Update([NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));

            var selectedIndex = 0;
            var index = 0;

            foreach (var pair in DataServiceManager.Types.OrderBy(t => t.Value.Priority))
            {
                DataDriver.Items.Add(pair.Key);

                if (pair.Key == site.DataServiceName)
                {
                    selectedIndex = index;
                }

                index++;
            }

            DataDriver.SelectedIndex = selectedIndex;

            var dataServiceEditor = DataServiceEditor;
            if (dataServiceEditor != null)
            {
                dataServiceEditor.Display(site);
            }
        }
    }
}
