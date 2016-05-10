// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command(Submenu = ViewSubmenu.Name), CommandId(CommandIds.ItemEditor.FieldDisplayTitles, typeof(ContentEditorContext)), ToolbarElement(typeof(ContentEditorContext), 9030, "View", "Show", Icon = "Resources/32x32/Plus.png", ElementType = RibbonElementType.CheckBox)]
    public class FieldDisplayTitles : CommandBase, IToolbarElement
    {
        public FieldDisplayTitles()
        {
            Text = Resources.Field_Display_Titles;
            Group = "View Options";
            SortingValue = 2500;
        }

        public override bool CanExecute(object parameter)
        {
            var options = AppHost.Settings.Options;
            IsChecked = options.ShowFieldDisplayTitles;

            return parameter is ContentEditorContext;
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
            options.ShowFieldDisplayTitles = !options.ShowFieldDisplayTitles;
            options.Save();

            context.ContentEditor.Refresh();
        }
    }
}
