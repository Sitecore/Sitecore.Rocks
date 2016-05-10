// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    [Command, ShellMenuCommand(CommandIds.PublishSubItems)]
    public class PublishSubItems : ContentTreeCommand
    {
        public PublishSubItems()
        {
            Type = typeof(Rocks.ContentTrees.Commands.Publishing.PublishSubItems);
        }
    }
}
