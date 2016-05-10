// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands.ContentEditors
{
    public abstract class ContentEditorCommand : CommandBase
    {
        protected ContentEditorCommand()
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

            var editor = ActiveContext.ActiveContentEditor;
            if (editor == null)
            {
                return false;
            }

            var type = Type;
            var context = GetContentEditorContext(editor);

            return CommandManager.QueryStatus(type, context);
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

            var type = Type;
            var context = GetContentEditorContext(editor);

            CommandManager.Execute(type, context);
        }

        [NotNull]
        protected virtual object GetContentEditorContext([NotNull] ContentEditor editor)
        {
            Debug.ArgumentNotNull(editor, nameof(editor));

            return new ContentEditorContext(editor);
        }

        [NotNull]
        protected virtual object GetContentTreeContext([NotNull] ContentTree tree)
        {
            Debug.ArgumentNotNull(tree, nameof(tree));

            return new ContentTreeContext(tree.ContentTreeView, tree.ContentTreeView.SelectedItems);
        }
    }
}
