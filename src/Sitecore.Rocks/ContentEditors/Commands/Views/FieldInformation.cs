// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command(Submenu = ViewSubmenu.Name), CommandId(CommandIds.ItemEditor.FieldInformation, typeof(ContentEditorContext)), ToolbarElement(typeof(ContentEditorContext), 9020, "View", "Show", Icon = "Resources/32x32/Plus.png", ElementType = RibbonElementType.CheckBox)]
    public class FieldInformation : CommandBase, IToolbarElement
    {
        public FieldInformation()
        {
            Text = Resources.Field_Information;
            Group = "View Options";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var options = AppHost.Settings.Options;
            IsChecked = options.ShowFieldInformation;

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
            options.ShowFieldInformation = !options.ShowFieldInformation;
            options.Save();

            context.ContentEditor.Refresh();
        }
    }
}
