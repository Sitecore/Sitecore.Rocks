// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    [Command, ShellMenuCommand(CommandIds.RebuildSearchIndex)]
    public class RebuildSearchIndex : ContentTreeCommand
    {
        public RebuildSearchIndex()
        {
            Type = typeof(Rocks.ContentTrees.Commands.Tools.RebuildSearchIndex);
        }
    }
}
