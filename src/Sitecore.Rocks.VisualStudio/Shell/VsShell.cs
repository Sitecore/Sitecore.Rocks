// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell.ShellCommands;

namespace Sitecore.Rocks.Shell
{
    public static class VsShell
    {
        private static readonly List<OleMenuCommand> menuCommands = new List<OleMenuCommand>();

        public static void RegisterShellMenuCommands([NotNull] SitecorePackage package)
        {
            Assert.ArgumentNotNull(package, nameof(package));

            menuCommands.Clear();

            var menuCommandService = package.MenuService;

            var context = new ShellContext();
            context.Initialize();

            var boundCommands = new List<BoundCommand>();

            var unboundCommands = new Dictionary<ShellMenuCommandPlacement, List<UnboundCommand>>();
            unboundCommands[ShellMenuCommandPlacement.MainMenu] = new List<UnboundCommand>();
            unboundCommands[ShellMenuCommandPlacement.SolutionExplorerItem] = new List<UnboundCommand>();
            unboundCommands[ShellMenuCommandPlacement.SolutionExplorerFolder] = new List<UnboundCommand>();
            unboundCommands[ShellMenuCommandPlacement.SolutionExplorerProject] = new List<UnboundCommand>();

            GetCommands(boundCommands, unboundCommands);

            CreateShellMenuCommands(context, menuCommandService, boundCommands);
            CreateShellMenuCommands(context, menuCommandService, unboundCommands, ShellMenuCommandPlacement.MainMenu, CommandIds.ExtensibilityMenu);
            CreateShellMenuCommands(context, menuCommandService, unboundCommands, ShellMenuCommandPlacement.SolutionExplorerItem, CommandIds.SolutionExplorerItemExtensibilityMenu);
            CreateShellMenuCommands(context, menuCommandService, unboundCommands, ShellMenuCommandPlacement.SolutionExplorerFolder, CommandIds.SolutionExplorerFolderExtensibilityMenu);
            CreateShellMenuCommands(context, menuCommandService, unboundCommands, ShellMenuCommandPlacement.SolutionExplorerProject, CommandIds.ProjectExtensibilityMenu);
        }

        public static void UnregisterShellMenuCommands([NotNull] SitecorePackage package)
        {
            Assert.ArgumentNotNull(package, nameof(package));

            var menuCommandService = package.MenuService;

            foreach (var command in menuCommands)
            {
                command.Visible = true;
                menuCommandService.RemoveCommand(command);
            }

            menuCommands.Clear();
        }

        private static void CreateShellMenuCommands([NotNull] ShellContext context, [NotNull] OleMenuCommandService menuCommandService, [NotNull] Dictionary<ShellMenuCommandPlacement, List<UnboundCommand>> unboundCommands, ShellMenuCommandPlacement placement, int cmdid)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(menuCommandService, nameof(menuCommandService));
            Debug.ArgumentNotNull(unboundCommands, nameof(unboundCommands));

            var commands = unboundCommands[placement];

            if (commands.Count == 0)
            {
                var commandId = new CommandID(GuidList.CommandSet, cmdid);

                var menuCommand = new OleMenuCommand(delegate { }, commandId, @"Extensibility Menu");
                menuCommand.BeforeQueryStatus += delegate { menuCommand.Visible = false; };

                menuCommandService.AddCommand(menuCommand);

                menuCommands.Add(menuCommand);

                return;
            }

            commands.Sort(new UnboundCommandSorter());

            foreach (var unboundShellCommand in commands)
            {
                var command = unboundShellCommand.Command;
                var commandId = new CommandID(GuidList.CommandSet, cmdid);

                EventHandler execute = delegate
                {
                    AppHost.Usage.ReportCommand(command, context);
                    command.Execute(context);
                };

                var menuCommand = new OleMenuCommand(execute, commandId, command.Text);

                EventHandler queryStatus = delegate
                {
                    try
                    {
                        menuCommand.Enabled = command.CanExecute(context);
                        menuCommand.Visible = command.IsVisible;
                    }
                    catch
                    {
                        menuCommand.Visible = false;
                        throw;
                    }
                };

                command.CanExecuteChanged += queryStatus;
                menuCommand.BeforeQueryStatus += queryStatus;

                menuCommandService.AddCommand(menuCommand);

                menuCommands.Add(menuCommand);

                cmdid++;
            }
        }

        private static void CreateShellMenuCommands([NotNull] ShellContext context, [NotNull] OleMenuCommandService menuCommandService, [NotNull] List<BoundCommand> boundCommands)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(menuCommandService, nameof(menuCommandService));
            Debug.ArgumentNotNull(boundCommands, nameof(boundCommands));

            foreach (var cmd in boundCommands)
            {
                var boundCommand = cmd;
                var command = boundCommand.Command;

                var commandId = new CommandID(GuidList.CommandSet, boundCommand.CommandId);
                var menuCommand = new OleMenuCommand((sender, args) => command.Execute(context), commandId, command.Text);

                EventHandler queryStatus = delegate
                {
                    try
                    {
                        menuCommand.Enabled = command.CanExecute(context);
                        menuCommand.Visible = command.IsVisible;
                    }
                    catch
                    {
                        menuCommand.Visible = false;
                        throw;
                    }
                };

                command.CanExecuteChanged += queryStatus;
                menuCommand.BeforeQueryStatus += queryStatus;

                menuCommandService.AddCommand(menuCommand);

                menuCommands.Add(menuCommand);
            }
        }

        private static void GetCommands([NotNull] List<BoundCommand> boundCommands, [NotNull] Dictionary<ShellMenuCommandPlacement, List<UnboundCommand>> unboundCommands)
        {
            Debug.ArgumentNotNull(boundCommands, nameof(boundCommands));
            Debug.ArgumentNotNull(unboundCommands, nameof(unboundCommands));

            var commands = CommandManager.Commands;
            foreach (var command in commands)
            {
                var attributes = command.GetType().GetCustomAttributes(typeof(CommandIdAttribute), true);

                foreach (CommandIdAttribute attribute in attributes)
                {
                    var commandId = attribute.CommandId;

                    if (boundCommands.Any(c => c.CommandId == commandId))
                    {
                        continue;
                    }

                    ICommand wrapper = null;

                    if (attribute.ExpectedCommandContext.Equals(typeof(ContentEditorContext)))
                    {
                        wrapper = new ContentEditorMenuCommand(command);
                    }
                    else if (attribute.ExpectedCommandContext.Equals(typeof(ContentTreeContext)))
                    {
                        wrapper = new ContentTreeMenuCommand(command);
                    }

                    if (wrapper == null)
                    {
                        continue;
                    }

                    var cmd = new BoundCommand
                    {
                        Command = wrapper,
                        CommandId = commandId
                    };

                    boundCommands.Add(cmd);
                }

                attributes = command.GetType().GetCustomAttributes(typeof(ShellMenuCommandAttribute), true);

                foreach (ShellMenuCommandAttribute attribute in attributes)
                {
                    if (attribute.IsBound)
                    {
                        var cmd = new BoundCommand
                        {
                            Command = command,
                            CommandId = attribute.CommandId
                        };

                        boundCommands.Add(cmd);
                    }
                    else
                    {
                        var cmd = new UnboundCommand
                        {
                            Command = command,
                            Priority = attribute.Priority
                        };

                        unboundCommands[attribute.Placement].Add(cmd);
                    }
                }
            }
        }

        private class BoundCommand
        {
            [NotNull]
            public ICommand Command { get; set; }

            public int CommandId { get; set; }
        }

        private class UnboundCommand
        {
            [NotNull]
            public ICommand Command { get; set; }

            public double Priority { get; set; }
        }

        private class UnboundCommandSorter : IComparer<UnboundCommand>
        {
            public int Compare([NotNull] UnboundCommand x, [NotNull] UnboundCommand y)
            {
                Assert.ArgumentNotNull(x, nameof(x));
                Assert.ArgumentNotNull(y, nameof(y));

                if (x.Priority > y.Priority)
                {
                    return 1;
                }

                if (x.Priority < y.Priority)
                {
                    return -1;
                }

                return x.Command.Text.CompareTo(y.Command.Text);
            }
        }
    }
}
