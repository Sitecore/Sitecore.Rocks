// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI.Libraries.ItemLibraries;
using Sitecore.Rocks.UI.Repositories;

namespace Sitecore.Rocks.UI.Libraries.Commands.Navigating
{
    [Command(Submenu = "AddToLibrary"), Feature(FeatureNames.Folders)]
    public class IncludeInNewLibrary : CommandBase
    {
        public IncludeInNewLibrary()
        {
            Text = "New Library...";
            Group = "New";
            SortingValue = 9000;
        }

        [CanBeNull]
        protected IEnumerable<IItem> Items { get; set; }

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

            var repository = RepositoryManager.GetRepository(RepositoryManager.Folders);
            var entry = repository.Entries.FirstOrDefault();
            if (entry == null)
            {
                return false;
            }

            Items = context.Items;

            return true;
        }

        public override void ContextMenuClosed()
        {
            Items = null;
        }

        public override void Execute(object parameter)
        {
            var items = Items;
            if (items == null)
            {
                return;
            }

            var folder = LibraryManager.AddNew((fileName, name) => new ItemLibrary(fileName, name)) as ItemLibrary;
            if (folder == null)
            {
                return;
            }

            folder.Add(items);
        }
    }
}
