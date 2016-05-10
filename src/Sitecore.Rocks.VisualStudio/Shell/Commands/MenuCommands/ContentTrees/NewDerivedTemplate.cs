// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    [Command, ShellMenuCommand(CommandIds.NewDerivedTemplate)]
    public class NewDerivedTemplate : ContentTreeCommand
    {
        public NewDerivedTemplate()
        {
            Type = typeof(Rocks.ContentTrees.Commands.Templates.NewDerivedTemplate);
        }
    }
}
