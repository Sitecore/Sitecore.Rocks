// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Commands
{
    [ExtensibilityInitialization(PreInit = @"Clear")]
    public static class CommandManager
    {
        private static readonly string CommandInterfaceName = typeof(ICommand).FullName;

        private static readonly List<CommandDescriptor> commands = new List<CommandDescriptor>();

        [NotNull]
        public static IEnumerable<ICommand> Commands
        {
            get
            {
                foreach (var instance in commands)
                {
                    yield return instance.Command;
                }
            }
        }

        [NotNull]
        internal static IEnumerable<CommandDescriptor> CommandDescriptors => commands;

        public static void Add([NotNull] CommandDescriptor commandDescriptor)
        {
            Assert.ArgumentNotNull(commandDescriptor, nameof(commandDescriptor));

            commands.Add(commandDescriptor);
        }

        [UsedImplicitly]
        public static void Clear()
        {
            commands.Clear();
        }

        public static bool Execute([NotNull] Type commandType, [CanBeNull] object parameter)
        {
            Assert.ArgumentNotNull(commandType, nameof(commandType));

            foreach (var command in commands)
            {
                if (!(command.Type == commandType))
                {
                    continue;
                }

                if (!command.Command.CanExecute(parameter))
                {
                    continue;
                }

                AppHost.Usage.ReportCommand(command.Command, parameter);
                command.Command.Execute(parameter);

                return true;
            }

            return false;
        }

        [CanBeNull]
        public static ICommand GetCommand([NotNull] string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            var descriptor = commands.FirstOrDefault(c => c.Command.GetType().FullName == typeName);

            return descriptor == null ? null : descriptor.Command;
        }

        [NotNull]
        public static IEnumerable<ICommand> GetCommands([NotNull] object context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return GetCommands(context, string.Empty);
        }

        [NotNull]
        public static IEnumerable<ICommand> GetCommands([NotNull] object context, [Localizable(false), NotNull] string submenu)
        {
            Assert.ArgumentNotNull(submenu, nameof(submenu));
            Assert.ArgumentNotNull(context, nameof(context));

            var result = new List<ICommand>();

            foreach (var command in commands)
            {
                if (command.Submenu != submenu)
                {
                    continue;
                }

                if (command.Command.CanExecute(context))
                {
                    result.Add(command.Command);
                }
            }

            result.Sort(SortCommands);

            return result;
        }

        [NotNull]
        public static string GetKeyText(Key key, ModifierKeys modifierKeys)
        {
            var result = key.ToString();

            if ((modifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                result = Resources.CommandManager_SetInputGestureText_Alt + @"+" + result;
            }

            if ((modifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                result = Resources.CommandManager_SetInputGestureText_Shift + @"+" + result;
            }

            if ((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
            {
                result = Resources.CommandManager_SetInputGestureText_Ctrl + @"+" + result;
            }

            if ((modifierKeys & ModifierKeys.Windows) == ModifierKeys.Windows)
            {
                result = Resources.CommandManager_GetKeyText_Win + @"+" + result;
            }

            return result;
        }

        public static bool HasCommands([NotNull] object context, [Localizable(false), NotNull] string subMenu)
        {
            Assert.ArgumentNotNull(subMenu, nameof(subMenu));
            Assert.ArgumentNotNull(context, nameof(context));

            return commands.Where(command => command.Submenu == subMenu).Any(command => command.Command.CanExecute(context));
        }

        public static bool QueryStatus([NotNull] Type commandType, [NotNull] object context)
        {
            Assert.ArgumentNotNull(commandType, nameof(commandType));
            Assert.ArgumentNotNull(context, nameof(context));

            foreach (var command in commands)
            {
                if (command.Type != commandType)
                {
                    continue;
                }

                if (command.Command.CanExecute(context))
                {
                    return true;
                }
            }

            return false;
        }

        [NotNull]
        public static IEnumerable<ICommand> Sort([NotNull] IEnumerable<ICommand> commands)
        {
            Assert.ArgumentNotNull(commands, nameof(commands));

            var result = new List<ICommand>(commands);

            result.Sort(SortCommands);

            return result;
        }

        public static void UnloadType([NotNull] Type commandType)
        {
            Assert.ArgumentNotNull(commandType, nameof(commandType));

            for (var i = commands.Count - 1; i >= 0; i--)
            {
                var descriptor = commands[i];

                if (descriptor.Type == commandType)
                {
                    commands.RemoveAt(i);
                }
            }
        }

        internal static void LoadType([NotNull] Type type, [NotNull] CommandAttribute attribute)
        {
            Debug.ArgumentNotNull(type, nameof(type));
            Debug.ArgumentNotNull(attribute, nameof(attribute));

            var i = type.GetInterface(CommandInterfaceName);
            if (i == null)
            {
                Trace.TraceError("Command has Command attribute but does not implement the ICommand interface");
                return;
            }

            ICommand command;
            try
            {
                var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null)
                {
                    return;
                }

                command = constructorInfo.Invoke(null) as ICommand;
            }
            catch
            {
                Trace.TraceError("Command threw an exception in the constructor");
                return;
            }

            if (command == null)
            {
                Trace.TraceError("Command does not have a parameterless constructor");
                return;
            }

            var commandInstance = new CommandDescriptor(command, attribute.Submenu, type)
            {
                ExcludeFromSearch = attribute.ExcludeFromSearch
            };

            commands.Add(commandInstance);
        }

        private static int SortCommands([NotNull] ICommand x, [NotNull] ICommand y)
        {
            Debug.ArgumentNotNull(x, nameof(x));
            Debug.ArgumentNotNull(y, nameof(y));

            var result = x.SortingValue - y.SortingValue;

            if (result == 0)
            {
                result = string.Compare(x.Text, y.Text, StringComparison.InvariantCultureIgnoreCase);
            }

            return result;
        }

        public class CommandDescriptor
        {
            public CommandDescriptor([NotNull] ICommand command, [NotNull] string subMenu, [NotNull] Type type)
            {
                Assert.ArgumentNotNull(command, nameof(command));
                Assert.ArgumentNotNull(subMenu, nameof(subMenu));
                Assert.ArgumentNotNull(type, nameof(type));

                Command = command;
                Submenu = subMenu;
                Type = type;
            }

            [NotNull]
            public ICommand Command { get; }

            public bool ExcludeFromSearch { get; internal set; }

            [NotNull]
            public string Submenu { get; }

            [NotNull]
            public Type Type { get; }

            public override string ToString()
            {
                var text = Command.Text;

                if (!string.IsNullOrEmpty(Submenu))
                {
                    text = Submenu + " | " + text;
                }

                return text;
            }
        }
    }
}
