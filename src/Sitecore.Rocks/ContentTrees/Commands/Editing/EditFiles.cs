// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command]
    public class EditFiles : CommandBase
    {
        public EditFiles()
        {
            Text = "Open";
            Group = "Edit";
            SortingValue = 1011;
            Icon = new Icon("Resources/16x16/pencil.png");
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

            if (!files.Any())
            {
                return false;
            }

            if (files.Any(f => f.FileUri.IsFolder))
            {
                return false;
            }

            if (files.Any(f => f.FileUri.BaseFolder == FileUriBaseFolder.Web && string.IsNullOrEmpty(f.FileUri.Site.WebRootPath)))
            {
                return false;
            }

            if (!files.Where(f => f.FileUri.BaseFolder == FileUriBaseFolder.Data).All(FileExists))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IFileSelectionContext;
            if (selection == null)
            {
                return;
            }

            foreach (var file in selection.Files)
            {
                if (file.FileUri.BaseFolder == FileUriBaseFolder.Data)
                {
                    var dataFile = file as FileTreeViewItem;
                    if (dataFile == null)
                    {
                        continue;
                    }

                    var dataFileName = dataFile.ServerFileName;
                    if (string.IsNullOrEmpty(dataFileName))
                    {
                        continue;
                    }

                    AppHost.Files.OpenFile(dataFileName);
                    continue;
                }

                var webRootPath = file.FileUri.Site.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    continue;
                }

                var fileName = Path.Combine(webRootPath, file.FileUri.RelativeFileName);
                if (File.Exists(fileName))
                {
                    AppHost.Files.OpenFile(fileName);
                }
            }
        }

        private bool FileExists([NotNull] IHasFileUri item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var fileTreeViewItem = item as FileTreeViewItem;
            if (fileTreeViewItem == null)
            {
                return false;
            }

            var fileName = fileTreeViewItem.ServerFileName;
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return File.Exists(fileName);
        }
    }
}
