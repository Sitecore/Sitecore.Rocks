// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    [Command, ShellMenuCommand(CommandIds.CollapseAll)]
    public class CollapseAll : ContentTreeCommand
    {
        public CollapseAll()
        {
            Type = typeof(Rocks.ContentTrees.Commands.Navigating.CollapseAll);
        }
    }
}
