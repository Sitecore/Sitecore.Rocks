// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command]
    public class OpenInWindowsExplorer : CommandBase
    {
        public OpenInWindowsExplorer()
        {
            Text = "Open in Windows Explorer";
            Group = "Edit";
            SortingValue = 1012;
        }

        public override bool CanExecute(object parameter)
        {
            if (parameter is ContentEditorContext)
            {
                return false;
            }

            var selection = parameter as IFileSelectionContext;
            if (selection == null)
            {
                return false;
            }

            var files = selection.Files.ToList();

            if (files.Count != 1)
            {
                return false;
            }

            if (files.Any(f => f.FileUri.IsFolder || string.IsNullOrEmpty(f.FileUri.Site.WebRootPath) || f.FileUri.BaseFolder == FileUriBaseFolder.Data))
            {
                return false;
            }

            return files.Any();
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IFileSelectionContext;
            if (selection == null)
            {
                return;
            }

            var file = selection.Files.FirstOrDefault();
            if (file == null)
            {
                return;
            }

            var fileName = Path.Combine(file.FileUri.Site.WebRootPath, file.FileUri.RelativeFileName);

            IO.File.OpenInWindowsExplorer(fileName);
        }
    }
}
