// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Skins.Default;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command(Submenu = ViewSubmenu.Name), CommandId(CommandIds.ItemEditor.ExpandSections, typeof(ContentEditorContext)), ToolbarElement(typeof(ContentEditorContext), 1940, "Home", "View", Icon = "Resources/16x16/Expand-All.png", ElementType = RibbonElementType.SmallButton)]
    public class ExpandSections : CommandBase, IToolbarElement
    {
        public ExpandSections()
        {
            Text = Resources.ExpandSections_ExpandSections_Expand_All_Sections;
            Group = "Sections";
            SortingValue = 500;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ContentEditorContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            var editor = context.ContentEditor.Editor;

            foreach (var findChild in editor.FindChildren<Section>())
            {
                findChild.Expander.IsExpanded = true;
            }
        }
    }
}
