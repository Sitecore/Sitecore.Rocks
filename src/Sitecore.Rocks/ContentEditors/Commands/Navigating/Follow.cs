// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentEditors.Commands.Navigating
{
    [Command]
    public class Follow : CommandBase
    {
        public Follow()
        {
            Text = Resources.Follow_Link;
            Group = "Navigate";
            SortingValue = 500;
            Icon = new Icon("Resources/16x16/follow.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;

            if (field.IsBlob)
            {
                return false;
            }

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return false;
            }

            var value = fieldControl.GetValue();
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            Guid id;
            if (!Guid.TryParse(value, out id))
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

            var value = fieldControl.GetValue();

            Guid id;
            if (!Guid.TryParse(value, out id))
            {
                return;
            }

            var field = context.Field.FieldUris.First();

            var itemId = new ItemId(id);

            var uri = new ItemVersionUri(new ItemUri(field.ItemVersionUri.ItemUri.DatabaseUri, itemId), field.Language, Data.Version.Latest);

            AppHost.OpenContentEditor(uri);
        }
    }
}
