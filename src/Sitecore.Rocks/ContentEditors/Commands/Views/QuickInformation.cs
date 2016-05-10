// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command(Submenu = ViewSubmenu.Name), CommandId(CommandIds.ItemEditor.QuickInformation, typeof(ContentEditorContext)), ToolbarElement(typeof(ContentEditorContext), 9030, "View", "Show", Icon = "Resources/32x32/Plus.png", ElementType = RibbonElementType.CheckBox)]
    public class QuickInformation : CommandBase, IToolbarElement
    {
        public QuickInformation()
        {
            Text = Resources.Quick_Information;
            Group = "View Options";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var options = AppHost.Settings.Options;
            IsChecked = !options.HideQuickInfo;

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

            options.HideQuickInfo = !options.HideQuickInfo;
            options.Save();

            context.ContentEditor.Refresh();
        }
    }
}
