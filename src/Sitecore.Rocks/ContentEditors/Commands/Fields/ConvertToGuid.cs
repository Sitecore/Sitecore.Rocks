// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields
{
    [Command]
    public class ConvertToGuid : CommandBase
    {
        public ConvertToGuid()
        {
            Text = "Convert to Guid";
            Group = "Convert";
            SortingValue = 700;
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

            return value.StartsWith("/sitecore/", StringComparison.InvariantCultureIgnoreCase);
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

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                Guid guid;
                if (!Guid.TryParse(response, out guid))
                {
                    AppHost.MessageBox("The item was not found.", Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                fieldControl.SetValue(guid.ToString("B").ToUpperInvariant());
            };

            field.Site.DataService.ExecuteAsync("Items.GetItemId", completed, value, field.DatabaseName.ToString());
        }
    }
}
