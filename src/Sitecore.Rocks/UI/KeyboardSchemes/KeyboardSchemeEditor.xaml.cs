// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.KeyboardSchemes
{
    public partial class KeyboardSchemeEditor
    {
        [CanBeNull]
        private CollectionView _view;

        public KeyboardSchemeEditor()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        public List<KeyboardShortcut> Shortcuts { get; } = new List<KeyboardShortcut>();

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            foreach (var command in Commands.CommandManager.Commands.Where(c => c.SubmenuOpened == null && !(c is Submenu)).OrderBy(c => c.GetType().FullName))
            {
                var commandName = command.GetType().FullName;
                var keys = Enumerable.Empty<Tuple<ModifierKeys, Key>>();

                var shortcut = KeyboardManager.Shortcuts.FirstOrDefault(s => s.CommandName == commandName);
                if (shortcut != null)
                {
                    keys = shortcut.Keys;
                }

                var tuple = new KeyboardShortcut(commandName, keys);
                Shortcuts.Add(tuple);
            }

            ShortcutListView.ItemsSource = Shortcuts;

            EnableButtons();

            _view = CollectionViewSource.GetDefaultView(Shortcuts) as CollectionView;
            if (_view == null)
            {
                return;
            }

            _view.Filter = delegate(object o)
            {
                var itemHeader = o as KeyboardShortcut;
                return itemHeader != null && itemHeader.CommandName.IsFilterMatch(RenderinSelectorFilter.Text);
            };

            _view.Refresh();
        }

        private void EnableButtons()
        {
            KeysTextBox.IsEnabled = ShortcutListView.SelectedItem != null;
        }

        private void FilterTextChanged(object sender, EventArgs e)
        {
            if (_view != null)
            {
                _view.Refresh();
            }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                return;
            }

            e.Handled = true;

            var text = KeysTextBox.Text ?? string.Empty;

            if (e.Key == Key.Back)
            {
                var n = text.LastIndexOf(',');
                KeysTextBox.Text = n >= 0 ? text.Left(n) : string.Empty;
                return;
            }

            var modifierKeys = Keyboard.Modifiers;

            if (modifierKeys == ModifierKeys.None && ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9)))
            {
                return;
            }

            var key = e.Key;
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            switch (key)
            {
                case Key.LeftAlt:
                case Key.LeftCtrl:
                case Key.LeftShift:
                case Key.RightAlt:
                case Key.RightCtrl:
                case Key.RightShift:
                case Key.System:
                    return;
            }

            if (text.IndexOf(',') >= 0)
            {
                return;
            }

            var s = string.Empty;

            if ((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
            {
                s += (!string.IsNullOrEmpty(s) ? "+" : string.Empty) + "Ctrl";
            }

            if ((modifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                s += (!string.IsNullOrEmpty(s) ? "+" : string.Empty) + "Alt";
            }

            if ((modifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                s += (!string.IsNullOrEmpty(s) ? "+" : string.Empty) + "Shift";
            }

            s += (!string.IsNullOrEmpty(s) ? "+" : string.Empty) + key;

            text += (!string.IsNullOrEmpty(text) ? ", " : string.Empty) + s;

            KeysTextBox.Text = text;
        }

        private void SelectShortcut(object sender, SelectionChangedEventArgs e)
        {
            var shortcut = ShortcutListView.SelectedItem as KeyboardShortcut;
            if (shortcut == null)
            {
                CommandNameTextBox.Text = string.Empty;
                KeysTextBox.Text = string.Empty;

                return;
            }

            CommandNameTextBox.Text = shortcut.CommandName;
            KeysTextBox.Text = shortcut.FormattedKeys;

            IsValidImage.Visibility = Visibility.Collapsed;

            EnableButtons();
        }

        private void SetShortcut(object sender, TextChangedEventArgs e)
        {
            var shortcut = ShortcutListView.SelectedItem as KeyboardShortcut;
            if (shortcut == null)
            {
                return;
            }

            IsValidImage.Visibility = shortcut.TryParse(KeysTextBox.Text) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
