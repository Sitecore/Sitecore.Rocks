// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    [Command, ShellMenuCommand(CommandIds.RebuildPublishingDatabase)]
    public class RebuildPublishingDatabase : ContentTreeCommand
    {
        public RebuildPublishingDatabase()
        {
            Type = typeof(Rocks.ContentTrees.Commands.Publishing.RebuildPublishingDatabase);
        }
    }
}
