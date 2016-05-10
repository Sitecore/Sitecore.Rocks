// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = "Tools")]
    public class PublishSubmenu : Submenu
    {
        public const string Name = "Publish";

        public PublishSubmenu()
        {
            Text = Resources.PublishingSubmenu_PublishingSubmenu_Publishing;
            Group = "Tools";
            SortingValue = 1000;
            SubmenuName = Name;
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
