// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.UI.Thumbnails;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.ImageFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 100, "Thumbnail", "Thumbnail", Text = "Set", ElementType = RibbonElementType.LargeButton, Icon = "Resources/32x32/Photo.png"), Command]
    public class SetThumbnail : CommandBase, IToolbarElement
    {
        public SetThumbnail()
        {
            Text = "Set Thumbnail";
            Group = "Thumbnail";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (!(field.Control is ImageField))
            {
                return false;
            }

            if (string.Compare(field.Type, "thumbnail", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return false;
            }

            if (!context.ContentEditor.ContentModel.IsSingle)
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

            var firstItem = context.ContentEditor.ContentModel.FirstItem;

            var dialog = new CreateThumbnailDialog();

            dialog.Initialize(firstItem.ItemUri.DatabaseUri, firstItem.Name);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var fieldControl = context.Field.Control;
                if (fieldControl != null)
                {
                    fieldControl.SetValue(response);
                }

                context.ContentEditor.ContentModel.IsModified = true;
            };

            firstItem.ItemUri.Site.DataService.ExecuteAsync("Items.SetThumbnail", completed, firstItem.ItemUri.DatabaseName.ToString(), firstItem.ItemUri.ItemId.ToString(), context.Field.FieldUris.First().FieldId.ToString(), dialog.FileName, "1", dialog.X, dialog.Y, false);
        }
    }
}
