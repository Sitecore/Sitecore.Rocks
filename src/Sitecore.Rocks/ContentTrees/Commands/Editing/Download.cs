// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows.Forms;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command, CommandId(CommandIds.SitecoreExplorer.Download, typeof(ContentTreeContext))]
    public class Download : CommandBase
    {
        public Download()
        {
            Text = Resources.DownloadAttachment_DownloadAttachment_Download;
            Group = "Attachment";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var item = context.Items.First() as ITemplatedItem;
            if (item == null)
            {
                return false;
            }

            if (IdManager.GetTemplateType(item.TemplateId) != "media")
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.First() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            var name = item.Name;
            var filter = string.Format(@"{0}|*.*", Resources.All_files);
            var extension = item.TemplateName;

            if (!string.IsNullOrEmpty(extension))
            {
                name = name + @"." + extension.ToLowerInvariant();
                filter = string.Format(@"{0} {3}|*.{1}|{2}", extension.Capitalize(), extension.ToLowerInvariant(), filter, Resources.DownloadAttachment_Execute_files);
            }

            var dialog = new SaveFileDialog
            {
                Title = Resources.MediaManager_DownloadAttachment_Download,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = name,
                Filter = filter
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            MediaManager.DownloadAttachment(item.ItemUri, dialog.FileName);
        }
    }
}
