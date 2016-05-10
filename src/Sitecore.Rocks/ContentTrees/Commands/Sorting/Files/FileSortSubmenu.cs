// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting.Files
{
    [Command(ExcludeFromSearch = true)]
    public class FileSortSubmenu : CommandBase
    {
        public FileSortSubmenu()
        {
            Text = "Sorting";
            Group = "Sorting";
            SortingValue = 2000;
            Icon = new Icon("Resources/16x16/sorting.png");
        }

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

            var item = context.SelectedItems.FirstOrDefault() as FileTreeViewItem;
            if (item == null)
            {
                return false;
            }

            if (!item.FileUri.IsFolder)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var item = context.SelectedItems.FirstOrDefault() as FileTreeViewItem;
            if (item == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            var sorterName = FileSortManager.GetSorterName(item.FileUri);

            var result = new List<ICommand>(CommandManager.GetCommands(parameter, "FileSorting"));

            foreach (var name in FileSortManager.Names)
            {
                var command = new SetFileSort
                {
                    SorterName = name,
                    Text = name,
                    IsChecked = name == sorterName
                };

                result.Add(command);
            }

            return result;
        }
    }
}
