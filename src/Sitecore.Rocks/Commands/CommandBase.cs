// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Commands
{
    public abstract class CommandBase : ICommand
    {
        private readonly List<ICommand> _subcommands = new List<ICommand>();

        private string _group;

        private string _shortcut;

        private string _text;

        protected CommandBase()
        {
            IsVisible = true;
            ToolTip = string.Empty;
        }

        [Localizable(false)]
        public string Group
        {
            get { return _group ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _group = value;
            }
        }

        public IIcon Icon { get; set; }

        public string InputGestureText
        {
            get { return _shortcut ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _shortcut = value;
            }
        }

        public bool IsChecked { get; set; }

        public bool IsVisible { get; set; }

        public int SortingValue { get; set; }

        public RoutedEventHandler SubmenuOpened { get; set; }

        [Localizable(true)]
        public string Text
        {
            get { return _text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _text = value;
            }
        }

        public string ToolTip { get; protected set; }

        public abstract bool CanExecute([CanBeNull] object parameter);

        public event EventHandler CanExecuteChanged;

        public virtual void ContextMenuClosed()
        {
        }

        public abstract void Execute([CanBeNull] object parameter);

        public virtual IEnumerable<ICommand> GetSubmenuCommands([CanBeNull] object parameter)
        {
            yield break;
        }

        protected internal void RaiseCanExecuteChanged()
        {
            var changed = CanExecuteChanged;
            if (changed != null)
            {
                changed(this, EventArgs.Empty);
            }
        }

        [UsedImplicitly]
        protected void Add([NotNull] ICommand command)
        {
            Debug.ArgumentNotNull(command, nameof(command));

            _subcommands.Add(command);
        }

        [UsedImplicitly]
        protected void AddRange([NotNull] IEnumerable<ICommand> commands)
        {
            Debug.ArgumentNotNull(commands, nameof(commands));

            _subcommands.AddRange(commands);
        }

        [UsedImplicitly]
        protected void Clear()
        {
            _subcommands.Clear();
        }

        [UsedImplicitly]
        protected void Remove([NotNull] ICommand command)
        {
            Debug.ArgumentNotNull(command, nameof(command));

            _subcommands.Remove(command);
        }
    }
}
