// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Repositories.Commands
{
    [Command]
    public class DeleteFile : CommandBase
    {
        public DeleteFile()
        {
            Text = "Delete File...";
            Group = "File";
            SortingValue = 3000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IRepositorySelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.FileNames.Any())
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IRepositorySelectionContext;
            if (context == null)
            {
                return;
            }

            string text;
            if (context.FileNames.Count() == 1)
            {
                text = string.Format("Are you sure you want to delete \"{0}\"?", context.FileNames.First());
            }
            else
            {
                text = string.Format("Are you sure you want to delete these {0} files?", context.FileNames.Count());
            }

            if (AppHost.MessageBox(text, Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            foreach (var fileName in context.FileNames)
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception ex)
                {
                    AppHost.MessageBox(string.Format("Failed to delete {0}\n\n{1}", fileName, ex.Message), "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            context.RepositoryPanel.Refresh();
        }
    }
}
