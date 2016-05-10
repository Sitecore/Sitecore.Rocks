// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.Shell.Commands.MenuCommands
{
    public abstract class SharedCommand : CommandBase
    {
        protected SharedCommand()
        {
            ActiveContext.FocusedChanged += delegate { RaiseCanExecuteChanged(); };
            ActiveContext.SelectedItemsChanged += delegate { RaiseCanExecuteChanged(); };
        }

        [NotNull]
        protected Type ContentEditorType { get; set; }

        [NotNull]
        protected Type ContentTreeType { get; set; }

        public override bool CanExecute(object parameter)
        {
            if (!(parameter is ShellContext))
            {
                return false;
            }

            Type type;
            object context;

            switch (ActiveContext.Focused)
            {
                case Focused.ContentTree:
                    var tree = ActiveContext.ActiveContentTree;
                    if (tree == null)
                    {
                        return false;
                    }

                    type = ContentTreeType;
                    context = GetContentTreeContext(tree);
                    break;

                case Focused.ContentEditor:
                    var editor = ActiveContext.ActiveContentEditor;
                    if (editor == null)
                    {
                        return false;
                    }

                    type = ContentEditorType;
                    context = GetContentEditorContext(editor);
                    break;

                default:
                    return false;
            }

            return CommandManager.QueryStatus(type, context);
        }

        public override void Execute(object parameter)
        {
            if (!(parameter is ShellContext))
            {
                return;
            }

            Type type;
            object context;

            switch (ActiveContext.Focused)
            {
                case Focused.ContentTree:
                    var tree = ActiveContext.ActiveContentTree;
                    if (tree == null)
                    {
                        return;
                    }

                    type = ContentTreeType;
                    context = GetContentTreeContext(tree);
                    break;

                case Focused.ContentEditor:
                    var editor = ActiveContext.ActiveContentEditor;
                    if (editor == null)
                    {
                        return;
                    }

                    type = ContentEditorType;
                    context = GetContentEditorContext(editor);
                    break;

                default:
                    return;
            }

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
