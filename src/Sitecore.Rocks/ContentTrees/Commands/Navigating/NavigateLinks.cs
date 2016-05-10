// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Commands.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Navigating
{
    [Command(Submenu = "Navigate"), CommandId(CommandIds.SitecoreExplorer.NavigateLinks, typeof(ContentTreeContext)), ToolbarElement(typeof(IItemSelectionContext), 1030, "Home", "Navigate", Icon = "Resources/16x16/link.png", ElementType = RibbonElementType.SmallButton)]
    public class NavigateLinks : NavigateLinksCommand, IToolbarElement
    {
        public NavigateLinks()
        {
            Text = Resources.NavigateLinks_NavigateLinks_Links;
            Group = "Links";
            SortingValue = 3000;
        }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is IItemSelectionContext))
            {
                return false;
            }

            return base.CanExecute(parameter);
        }
    }
}
