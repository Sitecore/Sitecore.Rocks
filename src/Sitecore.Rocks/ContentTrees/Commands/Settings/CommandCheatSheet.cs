// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Settings
{
    public class CommandCheatSheet : CommandBase
    {
        public CommandCheatSheet()
        {
            Text = Resources.KeyboardCheatSheet_KeyboardCheatSheet_Command_Cheat_Sheet;
            Group = "Settings";
            SortingValue = 8500;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            var commands = GetCommands();
            var result = FormatCommands(commands);

            var fileName = Path.Combine(Path.GetTempPath(), @"commands.xml");
            fileName = IO.File.MakeUniqueFileName(fileName);

            IO.File.Save(fileName, result);

            AppHost.Files.OpenFile(fileName);

            File.Delete(fileName);
        }

        [NotNull]
        private string FormatCommands([NotNull] List<CommandContext> contexts)
        {
            Debug.ArgumentNotNull(contexts, nameof(contexts));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            output.WriteStartElement(@"commands");

            foreach (var context in contexts)
            {
                output.WriteStartElement(@"context");
                output.WriteAttributeString(@"name", context.Name);

                WriteCommands(output, context.Commands.OrderBy(c => c.SortingValue));

                foreach (var submenu in context.Submenus)
                {
                    output.WriteStartElement(@"submenu");
                    output.WriteAttributeString(@"name", submenu.Name);
                    WriteCommands(output, submenu.Commands.OrderBy(c => c.SortingValue));
                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private List<CommandContext> GetCommands()
        {
            var commandsContexts = new List<CommandContext>();

            foreach (var command in CommandManager.Commands.OrderBy(c => c.GetType().FullName).ThenBy(c => c.SortingValue))
            {
                if (string.IsNullOrEmpty(command.Text))
                {
                    continue;
                }

                var fullName = command.GetType().FullName;

                var nameSpace = fullName;
                var n = fullName.LastIndexOf(@".", StringComparison.Ordinal);
                if (n >= 0)
                {
                    nameSpace = nameSpace.Left(n);
                }

                var contextName = fullName;
                n = contextName.LastIndexOf(@".Commands.", StringComparison.InvariantCultureIgnoreCase);
                if (n >= 0)
                {
                    contextName = contextName.Left(n);
                }

                n = contextName.LastIndexOf(@".", StringComparison.Ordinal);
                if (n >= 0)
                {
                    contextName = contextName.Mid(n + 1);
                }

                var context = commandsContexts.FirstOrDefault(c => c.Name == contextName);
                if (context == null)
                {
                    context = new CommandContext
                    {
                        Name = contextName
                    };

                    commandsContexts.Add(context);
                }

                var cmd = new Command
                {
                    Text = command.Text,
                    FullName = fullName,
                    NameSpace = nameSpace,
                    SortingValue = command.SortingValue,
                    InputGestureText = command.InputGestureText
                };

                var submenuName = GetSubmenuName(command);

                if (string.IsNullOrEmpty(submenuName))
                {
                    context.Commands.Add(cmd);
                }
                else
                {
                    var submenu = context.Submenus.FirstOrDefault(s => s.Name == submenuName);
                    if (submenu == null)
                    {
                        submenu = new Submenu
                        {
                            Name = submenuName
                        };

                        context.Submenus.Add(submenu);
                    }

                    submenu.Commands.Add(cmd);
                }
            }

            return commandsContexts;
        }

        [NotNull]
        private string GetSubmenuName([NotNull] ICommand command)
        {
            Debug.ArgumentNotNull(command, nameof(command));

            var submenuName = string.Empty;

            var customAttributes = command.GetType().GetCustomAttributes(typeof(CommandAttribute), true);
            if (customAttributes.Length > 0)
            {
                var attribute = customAttributes[0] as CommandAttribute;

                if (attribute != null)
                {
                    submenuName = attribute.Submenu;
                }
            }

            return submenuName;
        }

        private void WriteCommands([NotNull] XmlTextWriter output, [NotNull] IEnumerable<Command> commands)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(commands, nameof(commands));

            foreach (var command in commands)
            {
                output.WriteStartElement(@"command");
                output.WriteAttributeString(@"name", command.Text);

                if (!string.IsNullOrEmpty(command.InputGestureText))
                {
                    output.WriteAttributeString(@"keys", command.InputGestureText);
                }

                output.WriteAttributeString(@"typename", command.FullName);
                output.WriteAttributeString(@"namespace", command.NameSpace);
                output.WriteEndElement();
            }
        }

        private class Command
        {
            [NotNull]
            public string FullName { get; set; }

            [NotNull]
            public string InputGestureText { get; set; }

            [NotNull]
            public string NameSpace { get; set; }

            public double SortingValue { get; set; }

            [NotNull]
            public string Text { get; set; }
        }

        private class CommandContext
        {
            public CommandContext()
            {
                Commands = new List<Command>();
                Submenus = new List<Submenu>();
            }

            [NotNull]
            public List<Command> Commands { get; }

            [NotNull]
            public string Name { get; set; }

            [NotNull]
            public List<Submenu> Submenus { get; }
        }

        private class Submenu
        {
            public Submenu()
            {
                Commands = new List<Command>();
            }

            [NotNull]
            public List<Command> Commands { get; }

            [NotNull]
            public string Name { get; set; }
        }
    }
}
