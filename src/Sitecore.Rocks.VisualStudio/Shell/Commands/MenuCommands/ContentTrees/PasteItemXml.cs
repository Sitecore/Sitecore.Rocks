// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    [Command, ShellMenuCommand(CommandIds.PasteItemXml)]
    public class PasteItemXml : ContentTreeCommand
    {
        public PasteItemXml()
        {
            Type = typeof(Rocks.ContentTrees.Commands.Exporting.PasteItemXml);
        }
    }
}
