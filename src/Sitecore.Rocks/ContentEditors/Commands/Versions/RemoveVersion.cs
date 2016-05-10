// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentEditors.Commands.Versions
{
    [Command(Submenu = "Versions"), CommandId(CommandIds.RemoveVersion, typeof(ContentEditorContext))]
    public class RemoveVersion : CommandBase
    {
        public RemoveVersion()
        {
            Text = Resources.Remove_Version;
            Group = "Version";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty || contentModel.IsMultiple)
            {
                return false;
            }

            if (contentModel.IsSingle && contentModel.FirstItem.Versions.Count == 0)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty || contentModel.IsMultiple)
            {
                return;
            }

            var item = contentModel.FirstItem;

            var deletedVersionUri = item.Uri;

            item.Uri.Site.DataService.RemoveVersion(item.Uri);

            var versionUri = new ItemVersionUri(item.Uri.ItemUri, item.Uri.Language, Version.Latest);
            AppHost.OpenContentEditor(versionUri);

            Notifications.RaiseVersionRemoved(this, deletedVersionUri, versionUri);
        }
    }
}
