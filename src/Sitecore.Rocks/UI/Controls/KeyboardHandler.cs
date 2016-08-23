// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.UI.KeyboardSchemes;

namespace Sitecore.Rocks.UI.Controls
{
    public class KeyboardHandler : Control
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(@"Target", typeof(DependencyObject), typeof(KeyboardHandler));

        private readonly List<Tuple<ModifierKeys, Key>> _buffer = new List<Tuple<ModifierKeys, Key>>();

        private IContextProvider _contextProvider;

        private bool _contextProviderResolved;

        public KeyboardHandler()
        {
            Loaded += ControlLoaded;
            UsePreviewEvent = false;
        }

        [CanBeNull]
        public DependencyObject Target
        {
            get { return (DependencyObject)GetValue(TargetProperty); }

            set { SetValue(TargetProperty, value); }
        }

        public bool UsePreviewEvent { get; set; }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            if (Target == null)
            {
                Target = this.GetAncestor<IContextProvider>() as DependencyObject;
            }

            if (Target == null)
            {
                Target = this.GetAncestor<UserControl>();
            }

            if (Target == null)
            {
                Target = this.GetAncestor<Window>();
            }

            if (Target == null)
            {
                return;
            }

            if (UsePreviewEvent)
            {
                Keyboard.AddPreviewKeyDownHandler(Target, HandleKeyDown);
            }
            else
            {
                Keyboard.AddKeyDownHandler(Target, HandleKeyDown);
            }
        }

        [CanBeNull]
        private IContextProvider GetContextProvider()
        {
            if (_contextProvider != null)
            {
                return _contextProvider;
            }

            if (_contextProviderResolved)
            {
                return null;
            }

            _contextProviderResolved = true;

            var control = this as FrameworkElement;
            while (control != null)
            {
                var provider = control as IContextProvider;
                if (provider != null)
                {
                    _contextProvider = provider;
                    return provider;
                }

                control = VisualTreeHelper.GetParent(control) as FrameworkElement;
            }

            return null;
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var modifierKeys = Keyboard.Modifiers;

            if (modifierKeys == ModifierKeys.None && ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9)))
            {
                _buffer.Clear();
                return;
            }

            var key = e.Key;
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            switch (key)
            {
                case Key.Tab:
                case Key.Escape:
                case Key.LeftAlt:
                case Key.LeftCtrl:
                case Key.LeftShift:
                case Key.RightAlt:
                case Key.RightCtrl:
                case Key.RightShift:
                case Key.System:
                    return;
            }

            while (_buffer.Count > 1)
            {
                _buffer.RemoveAt(0);
            }

            _buffer.Add(new Tuple<ModifierKeys, Key>(modifierKeys, key));

            var commandNames = KeyboardManager.GetCommandNames(_buffer).ToList();
            if (!commandNames.Any())
            {
                return;
            }

            var provider = GetContextProvider();
            if (provider == null)
            {
                return;
            }

            var context = provider.GetContext();
            if (context == null)
            {
                return;
            }

            foreach (var commandName in commandNames)
            {
                var command = Commands.CommandManager.GetCommand(commandName);
                if (command == null)
                {
                    continue;
                }

                if (!command.CanExecute(context))
                {
                    continue;
                }

                _buffer.Clear();

                AppHost.Usage.ReportCommand(command, context);

                KeyboardManager.IsActive++;
                try
                {
                    command.Execute(context);
                }
                finally
                {
                    KeyboardManager.IsActive--;
                }

                e.Handled = true;
                break;
            }
        }
    }
}
