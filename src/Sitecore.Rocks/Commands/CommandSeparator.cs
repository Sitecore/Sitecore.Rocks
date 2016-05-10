// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Commands
{
    public sealed class CommandSeparator : ICommand
    {
        public CommandSeparator(int sortingValue)
        {
            SortingValue = sortingValue;
        }

        public bool BeginGroup => false;

        [NotNull]
        public IEnumerable<ICommand> Commands
        {
            get { yield break; }
        }

        public string Group => string.Empty;

        public IIcon Icon => null;

        public string InputGestureText => string.Empty;

        public bool IsChecked => false;

        public bool IsVisible => true;

        public int SortingValue { get; set; }

        public RoutedEventHandler SubmenuOpened => null;

        public string Text => string.Empty;

        public string ToolTip => string.Empty;

        public bool CanExecute([CanBeNull] object parameter)
        {
            return false;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute([CanBeNull] object parameter)
        {
        }

        public IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            yield break;
        }

        internal void RaiseCanExecuteChanged()
        {
            var changed = CanExecuteChanged;
            if (changed != null)
            {
                changed(this, EventArgs.Empty);
            }
        }
    }
}
