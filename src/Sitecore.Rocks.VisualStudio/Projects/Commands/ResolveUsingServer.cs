// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.ResolveUsingServer)]
    public class ResolveUsingServer : ResolveCommand
    {
        public ResolveUsingServer()
        {
            ConflictResolution = ConflictResolution.UseServerVersion;
        }
    }
}
