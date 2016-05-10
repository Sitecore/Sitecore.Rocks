// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.DataSources
{
    [Command]
    public class DataSourceSubmenu : Submenu
    {
        public const string Name = "DataSource";

        public DataSourceSubmenu()
        {
            Text = "Data Sources";
            Group = "Rendering";
            SortingValue = 5000;
            SubmenuName = Name;
            ContextType = typeof(LayoutDesignerContext);
        }
    }
}
