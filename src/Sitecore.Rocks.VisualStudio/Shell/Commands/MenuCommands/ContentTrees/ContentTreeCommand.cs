// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentTrees
{
    public abstract class ContentTreeCommand : CommandBase
    {
        protected ContentTreeCommand()
        {
            ActiveContext.FocusedChanged += delegate { RaiseCanExecuteChanged(); };
            ActiveContext.SelectedItemsChanged += delegate { RaiseCanExecuteChanged(); };
        }

        [NotNull]
        protected Type Type { get; set; }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is ShellContext))
            {
                return false;
            }

            if (ActiveContext.Focused != Focused.ContentTree)
            {
                return false;
            }

            var tree = ActiveContext.ActiveContentTree;
            if (tree == null)
            {
                return false;
            }

            var context = GetContext(tree);

            return CommandManager.QueryStatus(Type, context);
        }

        public override void Execute(object parameter)
        {
            var tree = ActiveContext.ActiveContentTree;
            if (tree == null)
            {
                return;
            }

            var context = GetContext(tree);

            CommandManager.Execute(Type, context);
        }

        [NotNull]
        protected virtual object GetContext([NotNull] ContentTree tree)
        {
            Debug.ArgumentNotNull(tree, nameof(tree));

            return new ContentTreeContext(tree.ContentTreeView, tree.ContentTreeView.SelectedItems);
        }
    }
}
