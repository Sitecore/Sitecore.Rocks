// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    [Export(typeof(IMode))]
    public class CommandMode : ModeBase
    {
        public CommandMode([NotNull] Commandy commandy) : base(commandy)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));

            Name = "Execute Command";
            Alias = "c";

            Commands = CommandManager.CommandDescriptors.Where(c => c.Command.CanExecute(Commandy.Parameter) && !(c.Command is Submenu) && !c.ExcludeFromSearch).Select(c => new CommandHit(c.Command, GetText(c)));
            IsReady = true;
        }

        public override string Watermark
        {
            get { return "Command Name"; }
        }

        [NotNull]
        internal IEnumerable<CommandHit> Commands { get; set; }

        public override void Execute(Hit hit, object parameter)
        {
            Assert.ArgumentNotNull(hit, nameof(hit));
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var command = hit.Tag as CommandHit;
            if (command == null)
            {
                return;
            }

            AppHost.Usage.ReportCommand(command.Command, parameter);
            command.Command.Execute(parameter);
        }

        [NotNull]
        private string GetText([NotNull] CommandManager.CommandDescriptor commandDescriptor)
        {
            Debug.ArgumentNotNull(commandDescriptor, nameof(commandDescriptor));

            if (!string.IsNullOrEmpty(commandDescriptor.Submenu))
            {
                return commandDescriptor.Submenu + @" | " + commandDescriptor.Command.Text;
            }

            return commandDescriptor.Command.Text;
        }

        public class CommandHit
        {
            public CommandHit([NotNull] ICommand command, [NotNull] string text)
            {
                Assert.ArgumentNotNull(command, nameof(command));
                Assert.ArgumentNotNull(text, nameof(text));

                Text = text;
                Command = command;
            }

            [NotNull]
            public ICommand Command { get; }

            [NotNull]
            public string Text { get; private set; }
        }
    }
}
