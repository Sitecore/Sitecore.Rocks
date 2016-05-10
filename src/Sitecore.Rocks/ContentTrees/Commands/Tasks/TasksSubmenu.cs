// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command]
    public class TasksSubmenu : Submenu
    {
        public const string Name = "Tasks";

        public TasksSubmenu()
        {
            Text = Resources.TasksSubmenu_TasksSubmenu_Tasks;
            Group = "Tools";
            SortingValue = 4400;
            SubmenuName = Name;
            ContextType = typeof(IItemSelectionContext);
        }
    }
}
