// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentEditors.Commands.Versions
{
    [Command(Submenu = "Versions"), CommandId(CommandIds.AddVersion, typeof(ContentEditorContext))]
    public class AddVersion : CommandBase
    {
        public AddVersion()
        {
            Text = Resources.AddVersion;
            Group = "Version";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
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

            GetValueCompleted<Version> callback = delegate(Version value)
            {
                if (value == Version.Empty)
                {
                    AppHost.MessageBox(Resources.Failed_to_add_a_new_version, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newUri = new ItemVersionUri(item.Uri.ItemUri, item.Uri.Language, value);
                AppHost.OpenContentEditor(newUri);

                Notifications.RaiseVersionAdded(this, newUri);
            };

            item.Uri.Site.DataService.AddVersion(item.Uri, callback);
        }
    }
}
