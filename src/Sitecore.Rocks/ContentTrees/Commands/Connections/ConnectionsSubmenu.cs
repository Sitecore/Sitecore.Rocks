// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Connections
{
    [Command]
    public class ConnectionsSubmenu : Submenu
    {
        public const string Name = "Connections";

        public ConnectionsSubmenu()
        {
            Text = Resources.ConnectionsSubmenu_ConnectionsSubmenu_Connections;
            Group = "Tools";
            SortingValue = 1000;
            SubmenuName = Name;
            Icon = new Icon("Resources/16x16/server.png");
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
