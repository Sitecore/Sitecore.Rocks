// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    [Command, ShellMenuCommand(CommandIds.DisconnectSite)]
    public class DisconnectSite : ContentTreeCommand
    {
        public DisconnectSite()
        {
            Type = typeof(Rocks.ContentTrees.Commands.Connections.DisconnectSite);
        }
    }
}
