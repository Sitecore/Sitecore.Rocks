// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting.Files
{
    public class SetFileSort : CommandBase
    {
        public SetFileSort()
        {
            Group = "Sort";
        }

        [NotNull]
        public string SorterName { get; set; }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as FileTreeViewItem;
            if (item == null)
            {
                return;
            }

            if (item.HasChildren && !item.IsExpanded)
            {
                item.ExpandAndWait();
            }

            FileSortManager.SetFileSort(item.FileUri, SorterName);

            item.Refresh();
        }
    }
}
