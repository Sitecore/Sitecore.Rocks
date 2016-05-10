// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Controls.Dialogs.BrowseTypeName;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields
{
    [Command]
    public class BrowseTypeName : CommandBase
    {
        private static readonly FieldId ControllerFieldId = new FieldId(new Guid("{1A0AE537-291C-4CC8-B53F-7CA307A0FEF5}"));

        private static readonly FieldId ModelFieldId = new FieldId(new Guid("{EE9E23D2-181D-4172-A929-0E0B8D10313C}"));

        public BrowseTypeName()
        {
            Text = "Browse for Type and Assembly...";
            Group = "Type";
            SortingValue = 10;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            var fieldUri = field.FieldUris.FirstOrDefault();
            if (fieldUri == null)
            {
                return false;
            }

            if (!ProjectManager.Projects.Any())
            {
                return false;
            }

            if (fieldUri.FieldId == ControllerFieldId)
            {
                return true;
            }

            if (fieldUri.FieldId == ModelFieldId)
            {
                return true;
            }

            if (field.Name == "Type" && (string.Compare(field.Type, "Single-Line Text", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.Type, "text", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                return true;
            }

            return false;
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

            var dialog = new BrowseTypeNameDialog(fieldControl.GetValue());

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            fieldControl.SetValue(dialog.SelectedTypeName);
        }
    }
}
