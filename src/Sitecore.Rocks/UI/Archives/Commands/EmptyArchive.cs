// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.UI.Archives.Commands
{
    [Command]
    public class EmptyArchive : CommandBase
    {
        public EmptyArchive()
        {
            Text = Resources.Empty_Execute_Empty;
            Group = "Archive";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ArchiveContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ArchiveContext;
            if (context == null)
            {
                return;
            }

            if (AppHost.MessageBox(Resources.Empty_Execute_Are_you_sure_you_want_to_permanently_delete_all_items_from_the_archive_, Resources.Empty_Execute_Empty, MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
            {
                return;
            }

            var databaseUri = context.ArchiveViewer.DatabaseUri;

            ExecuteCompleted callback = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                context.ArchiveViewer.Clear();
                context.ArchiveViewer.GetArchivedItems(0);
            };

            databaseUri.Site.DataService.ExecuteAsync("Archives.EmptyArchive", callback, databaseUri.DatabaseName.ToString(), context.ArchiveViewer.ArchiveName);
        }
    }
}
