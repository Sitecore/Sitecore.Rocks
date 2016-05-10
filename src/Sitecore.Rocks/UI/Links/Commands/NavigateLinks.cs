// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Commands.Commands;

namespace Sitecore.Rocks.UI.Links.Commands
{
    [Command]
    public class NavigateLinks : NavigateLinksCommand
    {
        public NavigateLinks()
        {
            Text = Resources.NavigateLinks_NavigateLinks_Links;
            Group = "Navigate";
            SortingValue = 5200;
        }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is LinksContext))
            {
                return false;
            }

            return base.CanExecute(parameter);
        }
    }
}
