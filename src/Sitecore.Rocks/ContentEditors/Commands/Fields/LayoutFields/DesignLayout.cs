// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.UI.LayoutDesigners;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.LayoutFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 10, "Layout", "Layout", Text = "Design", Icon = "Resources/32x32/Window-Edit.png"), Command]
    public class DesignLayout : CommandBase, IToolbarElement
    {
        public DesignLayout()
        {
            Text = "Design Layout";
            Group = "Layouts";
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
            if (!(field.Control is LayoutField))
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

            var field = context.Field;
            var layoutField = field.Control as LayoutField;
            if (layoutField == null)
            {
                return;
            }

            var fieldUri = field.FieldUris.FirstOrDefault();
            if (fieldUri == null)
            {
                return;
            }

            var paneCaption = "Layout";
            var item = context.ContentEditor.ContentModel.Items.FirstOrDefault();
            if (item != null)
            {
                paneCaption = item.Name;
            }

            var designer = AppHost.OpenDocumentWindow<LayoutDesigner>("PageDesigner" + fieldUri.ItemVersionUri);
            if (designer != null)
            {
                designer.Initialize(paneCaption, field.FieldUris, field.Value);
            }
        }
    }
}
