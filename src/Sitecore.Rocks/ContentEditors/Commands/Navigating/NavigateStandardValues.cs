// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Navigating
{
    [Command(Submenu = "Navigate"), CommandId(CommandIds.ItemEditor.NavigateStandardValues, typeof(ContentEditorContext), Text = "Navigate to Standard Values", ToolBar = "SitecoreItemEditor", Group = "Navigate", Icon = "Resources/16x16/standardvalues_navigate.png", Priority = 0x0110), ToolbarElement(typeof(ContentEditorContext), 1020, "Home", "Navigate", Icon = "Resources/16x16/standardvalues_navigate.png", Text = "Standard Values", ElementType = RibbonElementType.SmallButton)]
    public class NavigateStandardValues : CommandBase, IToolbarElement
    {
        public NavigateStandardValues()
        {
            Text = Resources.Standard_Values;
            Group = "Navigate";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty || contentModel.IsMultiple)
            {
                return false;
            }

            if (ActiveContext.ActiveContentTree == null)
            {
                return false;
            }

            var item = contentModel.FirstItem;
            if (item.StandardValuesId == ItemId.Empty)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty || contentModel.IsMultiple)
            {
                return;
            }

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree == null)
            {
                return;
            }

            var item = contentModel.FirstItem;

            var standardValueUri = new ItemUri(item.Uri.ItemUri.DatabaseUri, item.StandardValuesId);

            contentTree.Locate(standardValueUri);
        }
    }
}
