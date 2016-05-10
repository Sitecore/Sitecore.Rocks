// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields
{
    [Command]
    public class ConvertToItemPath : CommandBase
    {
        public ConvertToItemPath()
        {
            Text = "Convert to Item Path";
            Group = "Convert";
            SortingValue = 710;
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
            if (!value.StartsWith(@"{") || !value.EndsWith(@"}"))
            {
                return false;
            }

            Guid guid;
            return Guid.TryParse(value, out guid);
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
            var field = context.Field.FieldUris.First();

            Guid guid;
            if (!Guid.TryParse(value, out guid))
            {
                return;
            }

            var itemUri = new ItemUri(field.DatabaseUri, new ItemId(guid));

            AppHost.Server.GetItemHeader(itemUri, itemHeader => fieldControl.SetValue(itemHeader.Path));
        }
    }
}
