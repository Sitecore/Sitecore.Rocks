// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Commands.PlaceHolders
{
    [Command(ExcludeFromSearch = true)]
    public class AddSubmenu : CommandBase
    {
        public const string Name = "Add";

        public AddSubmenu()
        {
            Text = Resources.Add_Add_Add;
            Group = "Add";
            SortingValue = 5300;

            SubmenuOpened = Opened;
        }

        [CanBeNull]
        protected LayoutTreeViewContext Context { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutTreeViewContext;
            if (context == null)
            {
                return false;
            }

            if (!context.TreeViewItems.All(i => i is PlaceHolderTreeViewItem))
            {
                return false;
            }

            if (context.TreeViewItems.Count() != 1)
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

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            return CommandManager.GetCommands(parameter, Name);
        }

        private void CreateItem([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = Context;
            if (context == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var itemHeader = menuItem.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            var placeHolderTreeViewItem = context.TreeViewItems.FirstOrDefault() as PlaceHolderTreeViewItem;
            if (placeHolderTreeViewItem == null)
            {
                return;
            }

            placeHolderTreeViewItem.DeviceTreeViewItem.AddRendering(placeHolderTreeViewItem, itemHeader, -1, -1);
        }

        private void LoadRenderings([NotNull] MenuItem menuItem, [NotNull] IEnumerable<ItemHeader> renderings)
        {
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));
            Debug.ArgumentNotNull(renderings, nameof(renderings));

            var index = -1;

            var loading = menuItem.Items.OfType<MenuItem>().FirstOrDefault(i => i.Tag as string == @"loading");
            if (loading != null)
            {
                if (!renderings.Any())
                {
                    loading.Header = "None";
                    return;
                }

                index = menuItem.Items.IndexOf(loading);
                menuItem.Items.RemoveAt(index);
            }

            foreach (var itemHeader in renderings.OrderBy(r => r.Name).Reverse())
            {
                var item = new MenuItem
                {
                    Header = itemHeader.Name,
                    Tag = itemHeader
                };

                item.Click += CreateItem;

                if (index < 0)
                {
                    menuItem.Items.Add(item);
                }
                else
                {
                    menuItem.Items.Insert(index, item);
                }
            }
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

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var item = menuItem.Items.OfType<MenuItem>().FirstOrDefault(i => i.Tag as string == @"loading");
            if (item == null)
            {
                return;
            }

            var placeHolderTreeViewItem = context.TreeViewItems.FirstOrDefault() as PlaceHolderTreeViewItem;
            if (placeHolderTreeViewItem == null)
            {
                return;
            }

            var placeHolderName = placeHolderTreeViewItem.PlaceHolderName;
            var placeHolderPath = placeHolderTreeViewItem.GetPlaceHolderPath();

            var index = menuItem.Items.IndexOf(item);
            menuItem.Items.Insert(index + 1, new Separator());

            var device = placeHolderTreeViewItem.DeviceTreeViewItem.Device;

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var items = root.Elements().Select(element => ItemHeader.Parse(device.DatabaseUri, element)).ToList();

                LoadRenderings(menuItem, items);
            };

            AppHost.Server.LayoutBuilders.GetRenderingsInPlaceHolder(device.DatabaseUri, placeHolderName, placeHolderPath, completed);
        }
    }
}
