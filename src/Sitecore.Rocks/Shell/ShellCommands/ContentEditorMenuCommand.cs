// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.Shell.ShellCommands
{
    public class ContentEditorMenuCommand : CommandBase
    {
        public ContentEditorMenuCommand([NotNull] ICommand command)
        {
            Assert.ArgumentNotNull(command, nameof(command));

            Command = command;
            Text = command.Text;
        }

        [NotNull]
        protected ICommand Command { get; }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is ShellContext))
            {
                return false;
            }

            var editor = ActiveContext.ActiveContentEditor;
            if (editor == null)
            {
                return false;
            }

            return Command.CanExecute(new ContentEditorContext(editor));
        }

        public override void Execute(object parameter)
        {
            if (!(parameter is ShellContext))
            {
                return;
            }

            var editor = ActiveContext.ActiveContentEditor;
            if (editor == null)
            {
                return;
            }

            var context = new ContentEditorContext(editor);

            AppHost.Usage.ReportCommand(Command, context);
            Command.Execute(context);
        }
    }
}
