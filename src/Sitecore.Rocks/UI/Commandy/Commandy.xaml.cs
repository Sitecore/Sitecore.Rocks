// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.UI.Commandy.Modes;
using Sitecore.Rocks.UI.Commandy.Searchers;

namespace Sitecore.Rocks.UI.Commandy
{
    public partial class Commandy
    {
        [NotNull]
        private readonly Stack<IMode> stack = new Stack<IMode>();

        public Commandy([NotNull] Popup popup, [NotNull] object context, [CanBeNull] IMode initialMode = null, bool allowOtherModes = true)
        {
            Assert.ArgumentNotNull(popup, nameof(popup));
            Assert.ArgumentNotNull(context, nameof(context));

            InitializeComponent();

            Popup = popup;
            Parameter = context;
            Mode = initialMode;

            var parameters = new
            {
                Commandy = this
            };

            AppHost.Extensibility.ComposeParts(this, parameters);

            Loaded += ControlLoaded;

            if (allowOtherModes)
            {
                LoadModes();
            }
        }

        [CanBeNull]
        public IMode Mode { get; private set; }

        [NotNull, ImportMany(typeof(IMode))]
        public IEnumerable<IMode> Modes { get; protected set; }

        [NotNull]
        public object Parameter { get; set; }

        public static double PopupHorizontalOffset { get; set; }

        public static double PopupVerticalOffset { get; set; }

        [NotNull]
        protected Popup Popup { get; set; }

        [NotNull, ImportMany(typeof(ISearcher))]
        protected IEnumerable<ISearcher> Seachers { get; set; }

        public void AddHits([NotNull] IEnumerable<Hit> commands)
        {
            Assert.ArgumentNotNull(commands, nameof(commands));

            foreach (var command in commands.OrderBy(c => c.Rank).ThenBy(c => c.Text))
            {
                var listBoxItem = new ListBoxItem
                {
                    Content = command.Text,
                    Tag = command
                };

                CommandListBox.Items.Add(listBoxItem);
            }

            CommandListBox.Visibility = Visibility.Visible;
        }

        public static void Open([NotNull] object parameter, [CanBeNull] IMode initialMode = null, bool allowOtherModes = true)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var popup = new Popup();

            var grid = new Grid();

            var thumb = new Thumb
            {
                Width = 0,
                Height = 0,
            };

            popup.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    thumb.RaiseEvent(e);
                }
            };

            thumb.DragDelta += (sender, e) =>
            {
                popup.HorizontalOffset += e.HorizontalChange;
                popup.VerticalOffset += e.VerticalChange;
            };

            thumb.DragCompleted += delegate
            {
                PopupVerticalOffset = popup.VerticalOffset;
                PopupHorizontalOffset = popup.HorizontalOffset;
            };

            var commandy = new Commandy(popup, parameter, initialMode, allowOtherModes);

            grid.Children.Add(thumb);
            grid.Children.Add(commandy);

            popup.Child = grid;
            popup.StaysOpen = false;
            popup.AllowsTransparency = true;

            var verticalOffset = PopupVerticalOffset != 0 ? PopupVerticalOffset : (SystemParameters.FullPrimaryScreenHeight - 300) / 2;
            var horizontalOffset = PopupHorizontalOffset != 0 ? PopupHorizontalOffset : (SystemParameters.FullPrimaryScreenWidth - 300) / 2;

            popup.VerticalOffset = verticalOffset;
            popup.HorizontalOffset = horizontalOffset;

            popup.IsOpen = true;

            commandy.CommandTextBox.TextBox.Focus();
        }

        private void ApplyMode([NotNull] IMode mode)
        {
            Debug.ArgumentNotNull(mode, nameof(mode));

            Mode = mode;
            ModeNameTextBlock.Content = mode.Name + ":";

            CommandListBox.Items.Clear();
            CommandListBox.Visibility = Visibility.Collapsed;
            CommandTextBox.Text = string.Empty;
            CommandTextBox.Watermark = mode.Watermark;

            foreach (var searcher in Seachers)
            {
                searcher.SetActiveMode(mode);
            }

            Mode.IsReadyChanged += IsReady;

            if (Mode.IsReady)
            {
                Search();
            }
        }

        private void CancelContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void ClosePopup()
        {
            Popup.IsOpen = false;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            if (Mode == null)
            {
                IMode mode = null;

                var defaultCommandyMode = Parameter as IDefaultCommandyMode;
                if (defaultCommandyMode != null)
                {
                    mode = defaultCommandyMode.GetCommandyMode(this);
                }

                if (mode == null)
                {
                    mode = new CommandMode(this);
                }

                PushMode(mode);
                ApplyMode(mode);
            }

            Search();
        }

        private void Execute()
        {
            var selectedItem = CommandListBox.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var hit = selectedItem.Tag as Hit;
            if (hit == null)
            {
                return;
            }

            Execute(hit);
        }

        private void Execute([NotNull] Hit hit)
        {
            Debug.ArgumentNotNull(hit, nameof(hit));

            ClosePopup();

            if (Mode != null)
            {
                Mode.Execute(hit, Parameter);
            }
        }

        private void ExecuteSingleHit()
        {
            if (CommandListBox.Items.Count != 1)
            {
                return;
            }

            var selectedItem = CommandListBox.Items[0] as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var hit = selectedItem.Tag as Hit;
            if (hit == null)
            {
                return;
            }

            Execute(hit);
        }

        private void FocusListBox([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Enter)
            {
                ExecuteSingleHit();
                return;
            }

            if (e.Key != Key.Down)
            {
                return;
            }

            var selectedItem = CommandListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                CommandListBox.Focus();
                selectedItem.Focus();
                Keyboard.Focus(selectedItem);
                e.Handled = true;
                return;
            }

            selectedItem = CommandListBox.Items.OfType<ListBoxItem>().FirstOrDefault();
            if (selectedItem != null)
            {
                CommandListBox.Focus();
                selectedItem.Focus();
                Keyboard.Focus(selectedItem);
                e.Handled = true;
            }
        }

        private void FocusTextBox([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Escape)
            {
                ClosePopup();
                return;
            }

            if (e.Key == Key.Enter)
            {
                Execute();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back || e.Key == Key.Space || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Delete || (e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9))
            {
                CommandTextBox.TextBox.Focus();
                Keyboard.Focus(CommandTextBox.TextBox);
            }
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Escape)
            {
                ClosePopup();
                e.Handled = true;
            }
        }

        private void HandleModes([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Space)
            {
                SwitchMode(e);
            }

            if (e.Key == Key.Back)
            {
                Pop(e);
            }
        }

        private void HandleMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Execute();
        }

        private void IsReady([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Mode == null)
            {
                return;
            }

            if (!Mode.IsReady)
            {
                return;
            }

            if (CommandListBox.Items.Count == 0)
            {
                Search();
            }
        }

        private void LoadModes()
        {
            var menu = new StackPanel();

            foreach (var mode in Modes.OrderBy(m => m.Alias))
            {
                if (!mode.CanExecute(Parameter))
                {
                    continue;
                }

                var header = string.Format("{1}: {0}", mode.Name, mode.Alias);

                var menuItem = new MenuItem
                {
                    Header = header,
                    Tag = mode
                };

                menuItem.Click += SetMode;

                menu.Children.Add(menuItem);
            }

            ModesPopupButton.DropDownContent = menu;
        }

        private void Pop([NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            if (!string.IsNullOrEmpty(CommandTextBox.Text))
            {
                return;
            }

            e.Handled = true;

            if (stack.Count == 1)
            {
                return;
            }

            var mode = stack.Pop();
            mode.IsReadyChanged -= IsReady;

            ApplyMode(stack.Peek());
        }

        private void PushMode([NotNull] IMode mode)
        {
            Debug.ArgumentNotNull(mode, nameof(mode));

            stack.Push(mode);
        }

        private void Search([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Search();
        }

        private void Search()
        {
            CommandListBox.Items.Clear();
            CommandListBox.Visibility = Visibility.Collapsed;

            if (Mode == null)
            {
                return;
            }

            if (!Mode.IsReady)
            {
                return;
            }

            var context = new CommandyContext(this, Mode, CommandTextBox.Text);

            foreach (var searcher in Seachers)
            {
                searcher.Search(context);
            }
        }

        private void SetMode([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ModesPopupButton.IsOpen = false;

            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var mode = menuItem.Tag as IMode;
            if (mode == null)
            {
                return;
            }

            ApplyMode(mode);
        }

        private void SwitchMode([NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            if (Mode == null)
            {
                return;
            }

            var text = CommandTextBox.Text.Trim();

            var newMode = Mode.SwitchMode(text);
            if (newMode == null)
            {
                return;
            }

            if (!newMode.CanExecute(Parameter))
            {
                return;
            }

            UnapplyMode(Mode);
            PushMode(newMode);
            ApplyMode(newMode);

            e.Handled = true;
        }

        private void UnapplyMode([NotNull] IMode mode)
        {
            Debug.ArgumentNotNull(mode, nameof(mode));

            mode.IsReadyChanged -= IsReady;
        }
    }
}
