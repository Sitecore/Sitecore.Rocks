// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command, CommandId(CommandIds.ItemEditor.StandardFields, typeof(ContentEditorContext), ToolBar = ToolBars.ItemEditor.Id, Group = ToolBars.ItemEditor.View, Icon = "Resources/16x16/standardfields.png", Priority = 0x0800), ToolbarElement(typeof(ContentEditorContext), 1910, "Home", "View", ElementType = RibbonElementType.LargeToggleButton, Icon = "Resources/32x32/Header-and-Footer.png")]
    public class StandardFields : CommandBase, IToolbarElement
    {
        public StandardFields()
        {
            Text = Resources.Standard_Fields;
            Group = "View";
            SortingValue = 2100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty)
            {
                return false;
            }

            IsChecked = AppHost.Settings.Options.ShowStandardFields;

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
            if (contentModel.IsEmpty)
            {
                return;
            }

            AppHost.Settings.Options.ShowStandardFields = !AppHost.Settings.Options.ShowStandardFields;
            AppHost.Settings.Options.Save();

            context.ContentEditor.Refresh();
        }
    }
}
