// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Settings
{
    [Command(Submenu = ToolsSubmenu.Name), Feature(FeatureNames.Gutters)]
    public class GuttersSubmenu : CommandBase
    {
        public GuttersSubmenu()
        {
            Text = Resources.GuttersSubmenu_GuttersSubmenu_GutterSettings;
            Group = "Settings";
            SortingValue = 1000;
            SubmenuOpened = Opened;
        }

        protected ContentTreeContext Context { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if ((item.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
            {
                return false;
            }

            Context = context;

            return true;
        }

        public override void Execute(object parameter)
        {
        }

        private void LoadGutters([NotNull] MenuItem menuItem, [NotNull] string response)
        {
            Assert.ArgumentNotNull(menuItem, nameof(menuItem));
            Debug.ArgumentNotNull(response, nameof(response));

            menuItem.Items.Clear();

            if (string.IsNullOrEmpty(response))
            {
                var item = new MenuItem
                {
                    Header = Resources.GuttersSubmenu_LoadGutters__none_,
                    Foreground = SystemColors.GrayTextBrush
                };

                menuItem.Items.Add(item);

                return;
            }

            var reader = new StringReader(response);
            string line;

            do
            {
                line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var guid = line.Left(38);
                var isChecked = line.Mid(38, 1) == @"1";
                var header = line.Mid(39);

                var item = new MenuItem
                {
                    Header = header,
                    IsChecked = isChecked,
                    Tag = guid
                };

                item.Click += SetGutter;
                menuItem.Items.Add(item);
            }
            while (line != null);
        }

        private void Opened([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            var item = Context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            if (menuItem.Items.Count != 1)
            {
                return;
            }

            var m = menuItem.Items[0] as MenuItem;
            if (m == null || m.Tag as string != @"loading")
            {
                return;
            }

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                LoadGutters(menuItem, response);
            };

            item.Site.DataService.ExecuteAsync("GetGutters", callback, "core");
        }

        private void SetGutter([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = Context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var id = menuItem.Tag as string ?? string.Empty;

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                item.Refresh();
            };

            item.Site.DataService.ExecuteAsync("SetGutter", callback, id);
        }
    }
}
