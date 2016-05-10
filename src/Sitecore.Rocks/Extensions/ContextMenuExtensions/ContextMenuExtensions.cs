// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.IEnumerableExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Dialogs.SetShortDialogs;
using Sitecore.Rocks.UI.KeyboardSchemes;

namespace Sitecore.Rocks.Extensions.ContextMenuExtensions
{
    public static class ContextMenuExtensions
    {
        private static readonly List<string> Favorites = new List<string>();

        [NotNull]
        private static readonly Icon GreyStar;

        [NotNull]
        private static readonly Icon KeyboardIcon;

        [NotNull]
        private static readonly Icon YellowStar;

        static ContextMenuExtensions()
        {
            GreyStar = new Icon("Resources/16x16/star_hollow.png");
            YellowStar = new Icon("Resources/16x16/star_yellow.png");
            KeyboardIcon = new Icon("Resources/16x16/keyboard.png");

            Load();
        }

        [NotNull]
        public static ContextMenu Build([NotNull] this ContextMenu contextMenu, [NotNull] IEnumerable<Commands.ICommand> commands, [NotNull] object context)
        {
            Assert.ArgumentNotNull(contextMenu, nameof(contextMenu));
            Assert.ArgumentNotNull(commands, nameof(commands));
            Assert.ArgumentNotNull(context, nameof(context));

            var commandList = commands.ToList();
            if (!commandList.Any())
            {
                return contextMenu;
            }

            var isCustomizing = (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift);

            string group = null;
            var isSeparator = true;

            foreach (var command in commandList)
            {
                if (command is CommandSeparator)
                {
                    AddSeparator(contextMenu.Items, ref isSeparator);
                    continue;
                }

                if (string.Compare(command.Group, group, StringComparison.InvariantCulture) != 0)
                {
                    AddSeparator(contextMenu.Items, ref isSeparator);
                    group = command.Group;
                }

                isSeparator = false;

                var menuItem = GetMenuItem(command, context);
                contextMenu.Items.Add(menuItem);

                AddSubMenuItems(menuItem, command, context);
            }

            InsertCustomizedMenuItems(contextMenu, context);

            if (isCustomizing)
            {
                MakeCustomizable(contextMenu.Items, context);
                contextMenu.PreviewMouseUp += CustomizeMenuItem;
            }

            contextMenu.Closed += CloseContextMenu;

            return contextMenu;
        }

        [CanBeNull]
        public static ContextMenu GetContextMenu([NotNull] object context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var result = new ContextMenu();

            var commands = Commands.CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                return null;
            }

            result.Build(commands, context);

            return result;
        }

        [CanBeNull]
        public static ContextMenu GetContextMenu([NotNull] object context, [NotNull] ContextMenuEventArgs e)
        {
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(e, nameof(e));

            var result = GetContextMenu(context);
            if (result == null)
            {
                e.Handled = true;
            }

            return result;
        }

        private static void AddSeparator([NotNull] ItemCollection items, ref bool isSeparator)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            if (!isSeparator)
            {
                items.Add(new Separator());
            }

            isSeparator = true;
        }

        private static void AddSubMenuItems([NotNull] MenuItem menuItem, [NotNull] Commands.ICommand parentCommand, [NotNull] object context)
        {
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));
            Debug.ArgumentNotNull(parentCommand, nameof(parentCommand));
            Debug.ArgumentNotNull(context, nameof(context));

            var commands = parentCommand.GetSubmenuCommands(context);
            if (!commands.Any())
            {
                return;
            }

            commands = Commands.CommandManager.Sort(commands);

            string group = null;
            var isSeparator = true;

            foreach (var command in commands)
            {
                if (command is CommandSeparator)
                {
                    AddSeparator(menuItem.Items, ref isSeparator);
                    continue;
                }

                if (string.Compare(command.Group, group, StringComparison.InvariantCulture) != 0)
                {
                    AddSeparator(menuItem.Items, ref isSeparator);
                    group = command.Group;
                }

                isSeparator = false;

                var newItem = GetMenuItem(command, context);

                menuItem.Items.Add(newItem);

                AddSubMenuItems(newItem, command, context);
            }
        }

        private static void Close([NotNull] ItemsControl itemsControl)
        {
            Debug.ArgumentNotNull(itemsControl, nameof(itemsControl));

            foreach (var menuItem in itemsControl.Items.OfType<MenuItem>().NotNull())
            {
                var c = menuItem.Tag as CommandBase;
                if (c != null)
                {
                    c.ContextMenuClosed();
                }

                Close(menuItem);
            }
        }

        private static void CloseContextMenu([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var itemsControl = sender as ItemsControl;
            if (itemsControl != null)
            {
                Close(itemsControl);
            }
        }

        private static void CustomizeMenuItem([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var frameworkElement = e.Source as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }

            var image = frameworkElement as Image;
            if (image == null)
            {
                return;
            }

            var menuItem = frameworkElement.GetAncestorOrSelf<MenuItem>();
            if (menuItem == null)
            {
                return;
            }

            var tag = frameworkElement.Tag as string ?? string.Empty;

            if (tag.StartsWith("Shortcut:"))
            {
                SetShortcut(menuItem);
            }
            else
            {
                ToggleFavorite(menuItem);
            }
        }

        [NotNull]
        private static MenuItem GetMenuItem([NotNull] Commands.ICommand command, [NotNull] object context)
        {
            Debug.ArgumentNotNull(command, nameof(command));
            Debug.ArgumentNotNull(context, nameof(context));

            var text = command.Text;
            if (AppHost.Settings.Options.ShowGroupAndSortingValue)
            {
                text = string.Format(@"[{0} {1}]    {2}", command.SortingValue, command.Group, text);
            }

            var menuItem = new MenuItem
            {
                Header = text,
                Icon = command.Icon,
                IsChecked = command.IsChecked,
                Tag = command

                // InputGestureText = command.InputGestureText,
            };

            if (!string.IsNullOrEmpty(command.ToolTip))
            {
                menuItem.ToolTip = command.ToolTip;
            }

            var commandFullName = command.GetType().FullName;
            var shortcut = KeyboardManager.Shortcuts.FirstOrDefault(s => s.CommandName == commandFullName);
            if (shortcut != null)
            {
                menuItem.InputGestureText = shortcut.FormattedKeys;
            }

            menuItem.Click += delegate
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            };

            if (command.Icon != null)
            {
                menuItem.Icon = new Image
                {
                    Source = command.Icon.GetSource(),
                    Width = 16,
                    Height = 16
                };
            }

            if (command.SubmenuOpened != null)
            {
                var loading = new MenuItem
                {
                    Header = Resources.Loading,
                    Tag = @"loading",
                    Foreground = SystemColors.GrayTextBrush
                };

                menuItem.Items.Add(loading);
                menuItem.SubmenuOpened += command.SubmenuOpened;
            }

            return menuItem;
        }

        private static void InsertCustomizedMenuItems([NotNull] ContextMenu contextMenu, [NotNull] object context)
        {
            Debug.ArgumentNotNull(contextMenu, nameof(contextMenu));
            Debug.ArgumentNotNull(context, nameof(context));

            var menuItems = new List<MenuItem>();

            InsertCustomizedMenuItems(menuItems, contextMenu.Items, context);

            if (!menuItems.Any())
            {
                return;
            }

            contextMenu.Items.Insert(0, new Separator());

            menuItems.Reverse();

            foreach (var menuItem in menuItems)
            {
                contextMenu.Items.Insert(0, menuItem);
            }
        }

        private static void InsertCustomizedMenuItems([NotNull] List<MenuItem> menuItems, [NotNull] ItemCollection items, [NotNull] object context)
        {
            Debug.ArgumentNotNull(menuItems, nameof(menuItems));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(context, nameof(context));

            foreach (var menuItem in items.OfType<MenuItem>())
            {
                var command = menuItem.Tag as Commands.ICommand;
                if (command != null)
                {
                    var key = command.GetType().FullName + @"," + context.GetType().FullName;

                    if (Favorites.Contains(key))
                    {
                        menuItems.Add(GetMenuItem(command, context));
                    }
                }

                InsertCustomizedMenuItems(menuItems, menuItem.Items, context);
            }
        }

        private static void Load()
        {
            Favorites.Clear();

            var value = AppHost.Settings.GetString("Context Menus", "Customized Items", string.Empty);

            foreach (var s in value.Split('|'))
            {
                Favorites.Add(s);
            }
        }

        private static void MakeCustomizable([NotNull] ItemCollection items, [NotNull] object context)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(context, nameof(context));

            foreach (var menuItem in items.OfType<MenuItem>())
            {
                var command = menuItem.Tag as Commands.ICommand;

                if (command == null || menuItem.Items.Count > 0)
                {
                    var border = new Border
                    {
                        Child = new TextBlock(new Run(menuItem.Header as string ?? string.Empty))
                        {
                            Margin = new Thickness(40, 0, 0, 0)
                        }
                    };

                    menuItem.Header = border;
                }
                else
                {
                    var key = command.GetType().FullName + @"," + context.GetType().FullName;

                    var icon = Favorites.Contains(key) ? YellowStar : GreyStar;

                    var stack = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    var star = new Image
                    {
                        Source = icon.GetSource(),
                        Width = 16,
                        Height = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 4, 0),
                        Tag = key,
                        ToolTip = "Toggle favorite."
                    };

                    RenderOptions.SetBitmapScalingMode(star, BitmapScalingMode.NearestNeighbor);

                    stack.Children.Add(star);

                    if (!string.IsNullOrEmpty(command.InputGestureText))
                    {
                        star.Margin = new Thickness(0, 0, 24, 0);
                    }
                    else
                    {
                        var shortcut = new Image
                        {
                            Source = KeyboardIcon.GetSource(),
                            Width = 16,
                            Height = 16,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 4, 0),
                            Tag = "Shortcut: " + menuItem.Header + "|" + command.GetType().FullName + @"|" + context.GetType().FullName,
                            ToolTip = "Set keyboard shortcut."
                        };

                        RenderOptions.SetBitmapScalingMode(shortcut, BitmapScalingMode.NearestNeighbor);

                        stack.Children.Add(shortcut);
                    }

                    stack.Children.Add(new TextBlock
                    {
                        Text = menuItem.Header.ToString()
                    });

                    menuItem.Header = stack;
                }

                MakeCustomizable(menuItem.Items, context);
            }
        }

        private static void Save()
        {
            var value = new StringBuilder();

            foreach (var favorite in Favorites)
            {
                if (value.Length > 0)
                {
                    value.Append('|');
                }

                value.Append(favorite);
            }

            AppHost.Settings.Set("Context Menus", "Customized Items", value.ToString());
        }

        private static void SetShortcut([NotNull] MenuItem menuItem)
        {
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));

            var command = menuItem.Tag;
            if (command == null)
            {
                return;
            }

            var stack = menuItem.Header as StackPanel;
            if (stack == null)
            {
                return;
            }

            var image = stack.Children[1] as Image;
            if (image == null)
            {
                return;
            }

            var tag = image.Tag as string ?? string.Empty;

            tag = tag.Mid(10);

            var parts = tag.Split('|');

            var shortcut = KeyboardManager.Shortcuts.FirstOrDefault(s => s.CommandName == parts[1]);
            if (shortcut == null)
            {
                shortcut = new KeyboardShortcut(parts[0], string.Empty);
                KeyboardManager.Shortcuts.Add(shortcut);
            }

            var dialog = new SetShortcutDialog(shortcut, parts[0]);
            if (AppHost.Shell.ShowDialog(dialog) == true)
            {
                KeyboardManager.SaveActiveScheme();
            }
        }

        private static void ToggleFavorite([NotNull] MenuItem menuItem)
        {
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));

            var command = menuItem.Tag;
            if (command == null)
            {
                return;
            }

            var stack = menuItem.Header as StackPanel;
            if (stack == null)
            {
                return;
            }

            var image = stack.Children[0] as Image;
            if (image == null)
            {
                return;
            }

            var key = image.Tag as string ?? string.Empty;

            if (Favorites.Contains(key))
            {
                Favorites.Remove(key);
                image.Source = GreyStar.GetSource();
            }
            else
            {
                Favorites.Add(key);
                image.Source = YellowStar.GetSource();
            }

            Save();
        }
    }
}
