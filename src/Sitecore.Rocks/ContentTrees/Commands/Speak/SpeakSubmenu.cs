// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command(ExcludeFromSearch = true)]
    public class SpeakSubmenu : Submenu
    {
        public const string Name = "Speak";

        public SpeakSubmenu()
        {
            Text = "SPEAK";
            Group = "Speak";
            SortingValue = 50;
            SubmenuName = Name;
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
