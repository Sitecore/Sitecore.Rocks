// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.ContentTrees.Commands.WebAdministration
{
    [Command]
    public class WebServerSubmenu : Submenu
    {
        public const string Name = "Web Server";

        public WebServerSubmenu()
        {
            Text = "Web Server";
            Group = "Tools";
            SortingValue = 4700;
            SubmenuName = Name;
            ContextType = typeof(ContentTreeContext);
        }
    }
}
