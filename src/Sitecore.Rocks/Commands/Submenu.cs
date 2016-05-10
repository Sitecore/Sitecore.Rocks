// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Commands
{
    [Command]
    public abstract class Submenu : CommandBase
    {
        [CanBeNull]
        protected Type ContextType { get; set; }

        [NotNull, Localizable(false)]
        protected string SubmenuName { get; set; }

        public override bool CanExecute(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            if (!CanHandle(parameter))
            {
                return false;
            }

            return CommandManager.HasCommands(parameter, SubmenuName);
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            if (parameter == null)
            {
                return Enumerable.Empty<ICommand>();
            }

            if (!CanHandle(parameter))
            {
                return Enumerable.Empty<ICommand>();
            }

            return CommandManager.GetCommands(parameter, SubmenuName);
        }

        private bool CanHandle([NotNull] object parameter)
        {
            Debug.ArgumentNotNull(parameter, nameof(parameter));

            var type = parameter.GetType();

            if (ContextType.IsInterface)
            {
                if (type.GetInterface(ContextType.FullName) == null)
                {
                    return false;
                }
            }
            else if (type != ContextType && !type.IsSubclassOf(ContextType))
            {
                return false;
            }

            return true;
        }
    }
}
