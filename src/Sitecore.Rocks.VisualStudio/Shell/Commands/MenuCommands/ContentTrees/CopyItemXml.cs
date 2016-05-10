// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    [Command, ShellMenuCommand(CommandIds.CopyItemXml)]
    public class CopyItemXml : ContentTreeCommand
    {
        public CopyItemXml()
        {
            Type = typeof(Rocks.ContentTrees.Commands.Exporting.CopyItemXml);
        }
    }
}
