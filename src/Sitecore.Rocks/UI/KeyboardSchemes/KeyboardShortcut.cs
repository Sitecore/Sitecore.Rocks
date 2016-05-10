// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.KeyboardSchemes
{
    public class KeyboardShortcut : INotifyPropertyChanged
    {
        private IEnumerable<Tuple<ModifierKeys, Key>> _keys;

        public KeyboardShortcut([NotNull] string commandName, [NotNull] IEnumerable<Tuple<ModifierKeys, Key>> keys)
        {
            CommandName = commandName;
            _keys = keys;
        }

        public KeyboardShortcut([NotNull] string commandName, [NotNull] string keys)
        {
            CommandName = commandName;
            TryParse(keys);
        }

        public string CommandName { get; }

        public string FormattedKeys
        {
            get { return ToString(); }
            set
            {
                TryParse(value);
                OnPropertyChanged();
            }
        }

        public IEnumerable<Tuple<ModifierKeys, Key>> Keys
        {
            get { return _keys; }
            set
            {
                _keys = value;
                OnPropertyChanged(nameof(FormattedKeys));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            var keys = string.Empty;

            foreach (var tuple in _keys)
            {
                var s = string.Empty;

                if ((tuple.Item1 & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    s += (!string.IsNullOrEmpty(s) ? "+" : string.Empty) + "Ctrl";
                }

                if ((tuple.Item1 & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    s += (!string.IsNullOrEmpty(s) ? "+" : string.Empty) + "Alt";
                }

                if ((tuple.Item1 & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    s += (!string.IsNullOrEmpty(s) ? "+" : string.Empty) + "Shift";
                }

                s += (!string.IsNullOrEmpty(s) ? "+" : string.Empty) + tuple.Item2;

                keys += (!string.IsNullOrEmpty(keys) ? ", " : string.Empty) + s;
            }

            return keys;
        }

        public bool TryParse([NotNull] string value)
        {
            Keys = new List<Tuple<ModifierKeys, Key>>();

            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            var keyList = new List<Tuple<ModifierKeys, Key>>();
            var parts = value.Split(',').Select(k => k.Trim()).ToList();
            foreach (var part in parts)
            {
                var keyText = part;
                var modifierKeysText = string.Empty;

                var n = keyText.LastIndexOf('+');
                if (n >= 0)
                {
                    modifierKeysText = keyText.Left(n);
                    keyText = keyText.Mid(n + 1);
                }

                Key key;
                if (!Enum.TryParse(keyText, true, out key))
                {
                    return false;
                }

                var modifierKeys = ModifierKeys.None;
                var modifierKeyParts = modifierKeysText.Split('+');
                foreach (var modifierKeyPart in modifierKeyParts)
                {
                    switch (modifierKeyPart.ToLowerInvariant())
                    {
                        case "alt":
                            modifierKeys |= ModifierKeys.Alt;
                            break;
                        case "shift":
                            modifierKeys |= ModifierKeys.Shift;
                            break;
                        case "ctrl":
                            modifierKeys |= ModifierKeys.Control;
                            break;
                        case "win":
                            modifierKeys |= ModifierKeys.Windows;
                            break;
                        case "":
                            break;
                        default:
                            return false;
                    }
                }

                keyList.Add(new Tuple<ModifierKeys, Key>(modifierKeys, key));
            }

            Keys = keyList;
            return true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
