// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command, CommandId(CommandIds.ItemEditor.RawValues, typeof(ContentEditorContext), ToolBar = ToolBars.ItemEditor.Id, Group = ToolBars.ItemEditor.View, Icon = "Resources/16x16/rawvalues.png", Priority = 0x0810), ToolbarElement(typeof(ContentEditorContext), 1900, "Home", "View", ElementType = RibbonElementType.LargeToggleButton, Icon = "Resources/32x32/Format-Paragraph.png")]
    public class RawValues : CommandBase, IToolbarElement
    {
        public RawValues()
        {
            Text = Resources.Raw_Values;
            Group = "View";
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
            if (contentModel.IsEmpty)
            {
                return false;
            }

            IsChecked = AppHost.Settings.Options.ShowRawValues;

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

            var options = AppHost.Settings.Options;

            options.ShowRawValues = !options.ShowRawValues;
            options.Save();

            context.ContentEditor.Refresh();
        }
    }
}
