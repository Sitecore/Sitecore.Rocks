// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Tasks
{
    [Command]
    public class TasksSubmenu : Submenu
    {
        public const string Name = "Tasks";

        public TasksSubmenu()
        {
            Text = "Tasks";
            Group = "Tasks";
            SortingValue = 6000;
            SubmenuName = Name;
            ContextType = typeof(LayoutDesignerContext);
        }
    }
}
