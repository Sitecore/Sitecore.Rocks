// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.DataSourceFields
{
    [Command]
    public class ChangeFieldSource : CommandBase
    {
        public static readonly ItemId PathFieldId = new ItemId(new Guid("{1EB8AE32-E190-44A6-968D-ED904C794EBF}"));

        public ChangeFieldSource()
        {
            Text = "Change Field Source...";
            Group = "Edit";
            SortingValue = 100;
        }

        [CanBeNull]
        protected ContentEditorFieldContext Context { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (field.TemplateFieldId != PathFieldId)
            {
                return false;
            }

            Context = context;

            return true;
        }

        public override void ContextMenuClosed()
        {
            Context = null;
        }

        public override void Execute(object parameter)
        {
            var context = Context;
            if (context == null)
            {
                return;
            }

            var control = context.Field.Control;
            if (control == null)
            {
                return;
            }

            var field = new BuildSourceField(context.Field.Type, control.GetValue());

            var dialog = new BuildSourceDialog(context.Field.FieldUris.First().DatabaseUri, field, control.GetValue());
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            control.SetValue(dialog.DataSource);
        }
    }
}
