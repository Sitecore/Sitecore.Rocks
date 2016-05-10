// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Commands
{
    public interface ICommand : System.Windows.Input.ICommand
    {
        [NotNull]
        string Group { get; }

        [CanBeNull]
        IIcon Icon { get; }

        [NotNull]
        string InputGestureText { get; }

        bool IsChecked { get; }

        bool IsVisible { get; }

        int SortingValue { get; }

        [CanBeNull]
        RoutedEventHandler SubmenuOpened { get; }

        [NotNull]
        string Text { get; }

        [NotNull]
        string ToolTip { get; }

        [NotNull]
        IEnumerable<ICommand> GetSubmenuCommands([NotNull] object parameter);
    }
}
