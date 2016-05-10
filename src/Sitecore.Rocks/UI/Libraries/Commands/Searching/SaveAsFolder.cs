// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Searching;
using Sitecore.Rocks.UI.Libraries.SearchLibraries;

namespace Sitecore.Rocks.UI.Libraries.Commands.Searching
{
    [Command]
    public class SaveAsFolder : CommandBase
    {
        public SaveAsFolder()
        {
            Text = "Save as Library";
            Group = "Save";
            SortingValue = 6100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as SearchContext;
            if (context == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(context.SearchViewer.LastQueryText))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as SearchContext;
            if (context == null)
            {
                return;
            }

            var folder = LibraryManager.AddNew((fileName, name) => new SearchLibrary(fileName, name, context.SearchViewer.DatabaseUri, context.SearchViewer.LastQueryText)) as SearchLibrary;
            if (folder == null)
            {
                return;
            }

            folder.Save();
            folder.Initialize();
        }
    }
}
