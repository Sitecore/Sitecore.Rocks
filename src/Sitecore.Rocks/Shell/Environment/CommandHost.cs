// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class CommandHost : IEnumerable<ICommand>
    {
        public bool Execute([NotNull] System.Windows.Input.ICommand command, [NotNull] object context)
        {
            Assert.ArgumentNotNull(command, nameof(command));
            Assert.ArgumentNotNull(context, nameof(context));

            if (!command.CanExecute(context))
            {
                return false;
            }

            AppHost.Usage.ReportCommand(command, context);
            command.Execute(context);
            return true;
        }

        public bool Execute([NotNull] Type commandType, [NotNull] object context)
        {
            Assert.ArgumentNotNull(commandType, nameof(commandType));
            Assert.ArgumentNotNull(context, nameof(context));

            return CommandManager.Execute(commandType, context);
        }

        public IEnumerator<ICommand> GetEnumerator()
        {
            return CommandManager.Commands.GetEnumerator();
        }

        public void Register([NotNull] Type commandType, [CanBeNull] string submenu = null)
        {
            Assert.ArgumentNotNull(commandType, nameof(commandType));

            var attribute = new CommandAttribute();

            if (!string.IsNullOrEmpty(submenu))
            {
                attribute.Submenu = submenu;
            }

            CommandManager.LoadType(commandType, attribute);
        }

        public void Unregister([NotNull] Type commandType)
        {
            Assert.ArgumentNotNull(commandType, nameof(commandType));

            CommandManager.UnloadType(commandType);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return CommandManager.Commands.GetEnumerator();
        }
    }
}
