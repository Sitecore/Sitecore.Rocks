// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting
{
    [Command(Submenu = "Sorting"), CommandId(CommandIds.SitecoreExplorer.SubitemSorting, typeof(ContentTreeContext)), Feature(FeatureNames.Sorting)]
    public class SubitemSorting : CommandBase
    {
        public SubitemSorting()
        {
            Text = Resources.SubitemSorting_SubitemSorting_Subitem_Sorting;
            Group = "Subitem Sorting";
            SortingValue = 6000;
            SubmenuOpened = Opened;
        }

        [CanBeNull]
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

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if ((item.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.InsertOptions) != DataServiceFeatureCapabilities.InsertOptions)
            {
                return false;
            }

            Context = context;

            return true;
        }

        public override void ContextMenuClosed()
        {
            Context = null;
        }

        public override void Execute(object parameter)
        {
        }

        private void LoadSubitemSortings([NotNull] MenuItem menuItem, [NotNull] string response)
        {
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));
            Debug.ArgumentNotNull(response, nameof(response));

            menuItem.Items.Clear();

            if (string.IsNullOrEmpty(response))
            {
                var item = new MenuItem
                {
                    Header = Resources.SubitemSorting_LoadSubitemSortings__none_,
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

                item.Click += SetSubitemsSorting;
                menuItem.Items.Add(item);
            }
            while (line != null);
        }

        private void Opened([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = Context;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
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

                LoadSubitemSortings(menuItem, response);
            };

            item.ItemUri.Site.DataService.ExecuteAsync("GetSubitemSortings", callback, item.ItemUri.ItemId.ToString(), item.ItemUri.DatabaseName.Name);
        }

        private void SetSubitemsSorting([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = Context;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Subitems Sorting");

            var sortOrder = menuItem.Tag as string ?? string.Empty;

            ItemModifier.Edit(item.ItemUri, fieldId, sortOrder);

            item.Refresh();
        }
    }
}
