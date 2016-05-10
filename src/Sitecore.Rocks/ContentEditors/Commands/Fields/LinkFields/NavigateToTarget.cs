// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.LinkFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 5200, "Link", "Navigate", ElementType = RibbonElementType.LargeButton, Icon = "Resources/32x32/Nudge-Left.png"), Command(Submenu = "NavigateField")]
    public class NavigateToTarget : CommandBase, IToolbarElement
    {
        public NavigateToTarget()
        {
            Text = Resources.NavigateToTarget_NavigateToTarget_Target;
            Group = "Link";
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
            if (!(field.Control is LinkField))
            {
                return false;
            }

            if (AppHost.CurrentContentTree == null)
            {
                return false;
            }

            var linkField = (LinkField)field.Control;

            var itemUri = linkField.ItemUri;
            if (itemUri == ItemUri.Empty)
            {
                return false;
            }

            if (linkField.LinkType != "internal" && linkField.LinkType != "media")
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

            var linkField = (LinkField)context.Field.Control;
            if (linkField == null)
            {
                return;
            }

            var itemUri = linkField.ItemUri;
            if (itemUri == ItemUri.Empty)
            {
                return;
            }

            if (AppHost.CurrentContentTree != null)
            {
                AppHost.CurrentContentTree.Locate(itemUri);
            }
        }
    }
}
