// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command(Submenu = ViewSubmenu.Name), ToolbarElement(typeof(ContentEditorContext), 9100, "View", "Show", ElementType = RibbonElementType.CheckBox)]
    public class ShowRibbon : CommandBase, IToolbarElement
    {
        public ShowRibbon()
        {
            Text = "Ribbon";
            Group = "View Options";
            SortingValue = 2600;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            IsChecked = AppHost.Settings.GetBool("ContentEditor", "ShowRibbon", false);

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            AppHost.Settings.SetBool("ContentEditor", "ShowRibbon", !AppHost.Settings.GetBool("ContentEditor", "ShowRibbon", false));
            context.ContentEditor.ShowRibbon();
        }
    }
}
