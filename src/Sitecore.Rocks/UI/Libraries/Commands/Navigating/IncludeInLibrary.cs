// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;
using Sitecore.Rocks.UI.Libraries.ItemLibraries;

namespace Sitecore.Rocks.UI.Libraries.Commands.Navigating
{
    [Command(ExcludeFromSearch = true), Feature(FeatureNames.Folders)]
    public class IncludeInLibrary : CommandBase
    {
        public IncludeInLibrary()
        {
            Text = "Include in Library";
            Group = "Navigate";
            SortingValue = 5225;

            SubmenuOpened = Opened;
        }

        [CanBeNull]
        protected IItemSelectionContext Context { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            if (context.Items.Any(i => i is LibraryItemTreeViewItem))
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

            return CommandManager.GetCommands(parameter, "AddToLibrary");
        }

        private void AddToList([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = Context;
            if (context == null)
            {
                return;
            }

            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var itemLibrary = menuItem.Tag as ItemLibrary;
            if (itemLibrary == null)
            {
                return;
            }

            itemLibrary.Add(context.Items);
        }

        private void Opened([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            if (menuItem.Items.Count > 0)
            {
                menuItem.Items.Insert(0, new Separator());
            }

            var loading = menuItem.Items.OfType<MenuItem>().FirstOrDefault(i => i.Tag as string == @"loading");
            if (loading == null)
            {
                return;
            }

            var index = menuItem.Items.IndexOf(loading);
            menuItem.Items.RemoveAt(index);

            foreach (var itemLibrary in LibraryManager.Libraries.OfType<ItemLibrary>().OrderBy(w => w.Name).Reverse())
            {
                var m = new MenuItem
                {
                    Header = itemLibrary.Name,
                    Tag = itemLibrary
                };

                m.Click += AddToList;

                menuItem.Items.Insert(0, m);
            }
        }
    }
}
