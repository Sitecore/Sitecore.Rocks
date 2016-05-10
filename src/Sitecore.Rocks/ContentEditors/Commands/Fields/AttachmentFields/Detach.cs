// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.AttachmentFields
{
    [Command]
    public class Detach : CommandBase, IToolbarElement
    {
        public Detach()
        {
            Text = Resources.Clear;
            Group = "Edit";
            SortingValue = 2000;
            Icon = new Icon("Resources/16x16/delete.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var fieldControl = context.Field.Control;
            if (fieldControl == null)
            {
                return false;
            }

            var attachment = fieldControl as Attachment;
            if (attachment == null)
            {
                return false;
            }

            var value = fieldControl.GetValue();
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var fieldControl = context.Field.Control;
            if (fieldControl == null)
            {
                return;
            }

            var attachment = fieldControl as Attachment;
            if (attachment == null)
            {
                return;
            }

            GetValueCompleted<bool> detachCompleted = value => attachment.ReloadImage();

            MediaManager.Detach(context.Field.FieldUris.First().ItemVersionUri.ItemUri, detachCompleted);
        }
    }
}
