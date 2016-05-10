// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.KeyboardSchemes;

namespace Sitecore.Rocks.UI.Dialogs.SetShortDialogs
{
    public partial class SetShortcutDialog
    {
        public SetShortcutDialog([NotNull] KeyboardShortcut shortcut, [NotNull] string text)
        {
            Shortcut = shortcut;
            InitializeComponent();
            this.InitializeDialog();

            KeyLabel.Content = "Select keyboard shortcut for \"" + text + "\":";
            KeysTextBox.Text = shortcut.FormattedKeys;
        }

        protected KeyboardShortcut Shortcut { get; }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Shortcut.TryParse(KeysTextBox.Text);

            this.Close(true);
        }

        private void SetKey([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

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
    }
}
