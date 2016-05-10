// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.LinkFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 5210, "Link", "Navigate", ElementType = RibbonElementType.SmallButton), Command]
    public class NavigateLinkLinks : CommandBase, IToolbarElement
    {
        public NavigateLinkLinks()
        {
            Text = Resources.NavigateLinkLinks_NavigateLinkLinks_Target_Links;
            Group = "Navigate";
            SortingValue = 5200;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var linkField = context.Field.Control as LinkField;
            if (linkField == null)
            {
                return false;
            }

            if (linkField.ItemUri == ItemUri.Empty)
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

            var linkViewer = AppHost.Windows.Factory.OpenLinkViewer();
            if (linkViewer == null)
            {
                return;
            }

            var linkTab = linkViewer.CreateTab(linkField.ItemUri);

            linkTab.Initialize(linkField.ItemUri);
        }
    }
}
