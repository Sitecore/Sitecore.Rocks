// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensions.DateTimeExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.TempFiles.Commands
{
    public abstract class DeleteFiles : CommandBase
    {
        protected DateTime Timestamp { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TempFileViewerContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.TempFileViewer.FolderList.SelectedItems;
            if (selectedItems.Count <= 0)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TempFileViewerContext;
            if (context == null)
            {
                return;
            }

            string text;
            var selectedItems = context.TempFileViewer.FolderList.SelectedItems;

            if (selectedItems.Count == 1)
            {
                var fileFolder = (TempFileViewer.FileFolder)selectedItems[0];
                text = string.Format("Are you sure you want to delete all files in the \"{0}\" folder?", fileFolder.Folder);
            }
            else
            {
                text = string.Format("Are you sure you want to delete all files in these {0} folders?", selectedItems.Count);
            }

            if (AppHost.MessageBox(text, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var folders = new List<string>();
            foreach (var selectedItem in selectedItems)
            {
                var fileFolder = selectedItem as TempFileViewer.FileFolder;
                if (fileFolder != null)
                {
                    folders.Add(fileFolder.Name);
                }
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                context.TempFileViewer.LoadFileFolders();
            };

            context.TempFileViewer.Context.Site.DataService.ExecuteAsync("Files.DeleteTempFolders", completed, string.Join("|", folders), DateTimeExtensions.ToIsoDate(Timestamp));
        }
    }
}
