// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Exporting
{
    [Command(Submenu = "Tools")]
    public class XmlSubmenu : Submenu
    {
        public XmlSubmenu()
        {
            Text = Resources.XML;
            Group = "Exporting";
            SortingValue = 6000;
            SubmenuName = "XML";
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
