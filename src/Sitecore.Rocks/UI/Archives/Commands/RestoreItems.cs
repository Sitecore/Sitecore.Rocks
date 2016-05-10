// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Archives.Commands
{
    [Command]
    public class RestoreItems : CommandBase
    {
        public RestoreItems()
        {
            Text = Resources.Restore_Restore_Restore;
            Group = "Archive";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ArchiveContext;
            if (context == null)
            {
                return false;
            }

            if (!context.SelectedItems.Any())
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

            var selectedItems = GetSelectedItems(context);

            var databaseUri = context.ArchiveViewer.DatabaseUri;

            ExecuteCompleted callback = delegate(string response, ExecuteResult result)
            {
                DataService.HandleExecute(response, result);

                context.ArchiveViewer.Clear();
                context.ArchiveViewer.GetArchivedItems(0);
            };

            databaseUri.Site.DataService.ExecuteAsync("Archives.RestoreItems", callback, databaseUri.DatabaseName.ToString(), context.ArchiveViewer.ArchiveName, selectedItems);
        }

        [NotNull]
        private string GetSelectedItems([NotNull] ArchiveContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            var sb = new StringBuilder();
            var first = true;

            foreach (var selectedItem in context.SelectedItems)
            {
                if (!first)
                {
                    sb.Append('|');
                }
                else
                {
                    first = false;
                }

                sb.Append(selectedItem.Id.ToString("B").ToUpperInvariant());
            }

            return sb.ToString();
        }
    }
}
