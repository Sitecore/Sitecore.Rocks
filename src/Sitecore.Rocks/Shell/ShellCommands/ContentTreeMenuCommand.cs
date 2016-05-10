// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.Shell.ShellCommands
{
    public class ContentTreeMenuCommand : CommandBase
    {
        public ContentTreeMenuCommand([NotNull] ICommand command)
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

            var tree = ActiveContext.ActiveContentTree;
            if (tree == null)
            {
                return false;
            }

            return Command.CanExecute(new ContentTreeContext(tree.ItemTreeView, tree.ItemTreeView.SelectedItems));
        }

        public override void Execute(object parameter)
        {
            if (!(parameter is ShellContext))
            {
                return;
            }

            var tree = ActiveContext.ActiveContentTree;
            if (tree == null)
            {
                return;
            }

            var context = new ContentTreeContext(tree.ItemTreeView, tree.ItemTreeView.SelectedItems);
            AppHost.Usage.ReportCommand(Command, context);
            Command.Execute(context);
        }
    }
}
