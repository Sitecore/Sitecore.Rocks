// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fluent;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.Controls.Ribbons
{
    public partial class Ribbon
    {
        private readonly Dictionary<FrameworkElement, ICommand> _toolbarButtons = new Dictionary<FrameworkElement, ICommand>();

        public Ribbon()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public Func<ToolbarElement, bool> FilterElements { get; set; }

        private IContextProvider ContextProvider { get; set; }

        public void Render(object parameter)
        {
            FluentRibbon.Tabs.Clear();

            RenderTabs(parameter, false, tab => FluentRibbon.Tabs.Add(tab), element => true, () => ContextProvider != null ? ContextProvider.GetContext() : null);
        }

        public void RenderContextualTabs(string label, Brush color, object parameter)
        {
            FluentRibbon.ContextualGroups.Clear();
            for (var index = FluentRibbon.Tabs.Count - 1; index >= 0; index--)
            {
                var ribbonTabItem = FluentRibbon.Tabs[index];
                if (ribbonTabItem.Tag as string == "Contextual")
                {
                    FluentRibbon.Tabs.Remove(ribbonTabItem);
                }
            }

            var group = new RibbonContextualTabGroup();
            FluentRibbon.ContextualGroups.Add(group);

            group.Tag = label;
            group.Background = color;
            group.Visibility = Visibility.Visible;

            Func<ToolbarElement, bool> canRender = element =>
            {
                var command = element.Element as System.Windows.Input.ICommand;
                return command != null && command.CanExecute(parameter);
            };

            RenderTabs(parameter, true, delegate(RibbonTabItem tab)
            {
                tab.Background = Brushes.Green;
                tab.Tag = "Contextual";
                FluentRibbon.Tabs.Add(tab);
            }, canRender, () => parameter);
        }

        public void Update(IContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
            var parameter = ContextProvider.GetContext();

            Update(parameter);
        }

        public void Update([CanBeNull] object parameter)
        {
            foreach (var button in _toolbarButtons)
            {
                button.Key.IsEnabled = button.Value.CanExecute(parameter);

                var toggleButton = button.Key as ToggleButton;
                if (toggleButton != null)
                {
                    toggleButton.IsChecked = button.Value.IsChecked;
                }
            }
        }

        protected void RenderTabs(object parameter, bool isContextual, Action<RibbonTabItem> addTab, Func<ToolbarElement, bool> canRender, Func<object> getContext)
        {
            var elements = AppHost.Container.Get<ToolbarManager>().GetElements(parameter);

            if (FilterElements != null)
            {
                elements = elements.Where(e => FilterElements(e)).ToList();
            }

            string strip = null;
            string chunk = null;

            RibbonTabItem tab = null;
            RibbonGroupBox group = null;
            var buttonType = RibbonElementType.LargeButton;
            StackPanel stackPanel = null;
            var smallButtons = 0;
            var isTabSelected = !isContextual;

            foreach (var element in elements.OrderBy(c => c.SortOrder).ThenBy(c => c.Strip).ThenBy(c => c.Chunk).ThenBy(c => c.Text))
            {
                if (!canRender(element))
                {
                    continue;
                }

                if (element.Strip != strip)
                {
                    strip = element.Strip;

                    tab = new RibbonTabItem
                    {
                        Header = strip,
                        IsSelected = isTabSelected
                    };

                    addTab(tab);

                    isTabSelected = false;
                    chunk = null;
                    group = null;
                    stackPanel = null;
                    buttonType = RibbonElementType.LargeButton;
                }

                if (element.Chunk != chunk)
                {
                    chunk = element.Chunk;

                    group = new RibbonGroupBox
                    {
                        Header = chunk
                    };

                    tab.Groups.Add(group);
                    stackPanel = null;
                    buttonType = RibbonElementType.LargeButton;
                }

                var newButtonType = element.ElementType;
                if (newButtonType == RibbonElementType.CheckBox || newButtonType == RibbonElementType.SmallDropDown || newButtonType == RibbonElementType.SmallToggleButton)
                {
                    newButtonType = RibbonElementType.SmallButton;
                }

                if (newButtonType != buttonType)
                {
                    buttonType = newButtonType;
                    stackPanel = null;

                    if (buttonType == RibbonElementType.SmallButton)
                    {
                        stackPanel = new StackPanel
                        {
                            Margin = new Thickness(4, 0, 4, 0)
                        };

                        group.Items.Add(stackPanel);
                        smallButtons = 0;
                    }
                }

                UIElement control = null;
                switch (element.ElementType)
                {
                    case RibbonElementType.LargeButton:
                    case RibbonElementType.SmallButton:
                        control = CreateButton(parameter, element, isContextual, getContext);
                        break;
                    case RibbonElementType.SmallToggleButton:
                    case RibbonElementType.LargeToggleButton:
                        control = CreateToggleButton(parameter, element, isContextual, getContext);
                        break;
                    case RibbonElementType.LargeDropDown:
                    case RibbonElementType.SmallDropDown:
                        control = CreateDropDown(parameter, element, isContextual, getContext);
                        break;
                    case RibbonElementType.Gallery:
                        control = CreateGallery(parameter, element);
                        break;
                    case RibbonElementType.CheckBox:
                        control = CreateCheckbox(parameter, element, isContextual, getContext);
                        break;
                }

                if (control == null)
                {
                    continue;
                }

                if (stackPanel != null)
                {
                    if (smallButtons == 3)
                    {
                        stackPanel = new StackPanel();

                        group.Items.Add(stackPanel);
                        smallButtons = 0;
                    }

                    smallButtons++;
                    stackPanel.Children.Add(control);
                }
                else
                {
                    group.Items.Add(control);
                }
            }
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoaded;

            var appTheme = AppHost.Options.AppTheme;
            if (appTheme == AppTheme.System)
            {
                return;
            }

            if (appTheme == AppTheme.Automatic)
            {
                appTheme = AppHost.Shell.VisualStudioTheme;
            }

            if (appTheme != AppTheme.Dark)
            {
                return;
            }

            var dictionary = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Fluent;Component/Themes/Office2010/Black.xaml")
            };

            Resources.MergedDictionaries.Add(dictionary);
        }

        private FrameworkElement CreateButton(object parameter, ToolbarElement element, bool isContextual, Func<object> getContext)
        {
            var command = element.Element as ICommand;
            if (command == null)
            {
                return null;
            }

            BitmapImage iconSource = null;

            if (element.Icon != null || element.ElementType != RibbonElementType.SmallButton)
            {
                var icon = element.Icon != null ? new Icon(element.Icon) : Icon.Empty;
                iconSource = icon.GetSource();
            }

            var button = new Fluent.Button()
            {
                Header = string.IsNullOrEmpty(element.Text) ? command.Text : element.Text,
                IsEnabled = command.CanExecute(parameter),
                Icon = iconSource,
                LargeIcon = iconSource,
                SizeDefinition = element.ElementType == RibbonElementType.SmallButton ? "Middle" : "Large"
            };

            if (!string.IsNullOrEmpty(command.ToolTip))
            {
                button.ToolTip = command.ToolTip;
            }

            if (!string.IsNullOrEmpty(command.InputGestureText))
            {
                button.KeyTip = command.InputGestureText;
            }

            button.Click += delegate
            {
                var context = getContext();
                if (context == null)
                {
                    return;
                }

                if (command.CanExecute(context))
                {
                    command.Execute(context);
                }
            };

            if (!isContextual)
            {
                _toolbarButtons[button] = command;
            }

            return button;
        }

        private UIElement CreateCheckbox(object parameter, ToolbarElement element, bool isContextual, Func<object> getContext)
        {
            var command = element.Element as ICommand;
            if (command == null)
            {
                return null;
            }

            var checkBox = new Fluent.CheckBox
            {
                Header = !string.IsNullOrEmpty(element.Text) ? element.Text : command.Text,
                IsEnabled = command.CanExecute(parameter),
                IsChecked = command.IsChecked
            };

            if (!string.IsNullOrEmpty(command.ToolTip))
            {
                checkBox.ToolTip = command.ToolTip;
            }

            if (!string.IsNullOrEmpty(command.InputGestureText))
            {
                checkBox.KeyTip = command.InputGestureText;
            }

            checkBox.Click += delegate
            {
                var context = getContext();
                if (context == null)
                {
                    return;
                }

                if (command.CanExecute(context))
                {
                    command.Execute(context);
                }
            };

            if (!isContextual)
            {
                _toolbarButtons[checkBox] = command;
            }

            return checkBox;
        }

        private DropDownButton CreateDropDown(object parameter, ToolbarElement element, bool isContextual, Func<object> getContext)
        {
            var command = element.Element as ICommand;
            if (command == null)
            {
                return null;
            }

            var icon = Icon.Empty;
            if (element.Icon != null)
            {
                icon = new Icon(element.Icon);
            }

            var iconSource = icon.GetSource();

            var button = new DropDownButton
            {
                Header = !string.IsNullOrEmpty(element.Text) ? element.Text : command.Text,
                IsEnabled = command.CanExecute(parameter),
                Icon = iconSource,
                LargeIcon = iconSource,
            };

            button.DropDownOpened += delegate
            {
                button.Items.Clear();

                var submenuCommand = command;
                if (submenuCommand == null)
                {
                    return;
                }

                var context = getContext();
                if (context == null)
                {
                    return;
                }

                var commands = submenuCommand.GetSubmenuCommands(context).ToList();

                string group = null;
                foreach (var c in commands.OrderBy(c => c.SortingValue).ThenBy(c => c.Text))
                {
                    var cmd = c;

                    if (cmd.Group != group)
                    {
                        if (group != null)
                        {
                            button.Items.Add(new Separator());
                        }

                        group = cmd.Group;
                    }

                    var menuItem = new Fluent.MenuItem
                    {
                        Header = cmd.Text,
                        IsChecked = cmd.IsChecked
                    };

                    if (!string.IsNullOrEmpty(command.ToolTip))
                    {
                        menuItem.ToolTip = command.ToolTip;
                    }

                    if (!string.IsNullOrEmpty(command.InputGestureText))
                    {
                        menuItem.KeyTip = command.InputGestureText;
                    }

                    if (cmd.Icon != null && cmd.Icon != Icon.Empty)
                    {
                        menuItem.Icon = cmd.Icon.GetSource();
                    }

                    menuItem.Click += delegate
                    {
                        if (cmd.CanExecute(context))
                        {
                            cmd.Execute(context);
                        }
                    };

                    button.Items.Add(menuItem);
                }
            };

            if (!string.IsNullOrEmpty(command.ToolTip))
            {
                button.ToolTip = command.ToolTip;
            }

            if (!string.IsNullOrEmpty(command.InputGestureText))
            {
                button.KeyTip = command.InputGestureText;
            }

            if (!isContextual)
            {
                _toolbarButtons[button] = command;
            }

            return button;
        }

        private UIElement CreateGallery(object parameter, ToolbarElement element)
        {
            var gallery = element.Element as IToolbarGalleryFactory;
            if (gallery == null)
            {
                return null;
            }

            var result = new InRibbonGallery
            {
                Header = element.Text
            };

            gallery.Create(result, parameter);

            return result;
        }

        private FrameworkElement CreateToggleButton(object parameter, ToolbarElement element, bool isContextual, Func<object> getContext)
        {
            var command = element.Element as ICommand;
            if (command == null)
            {
                return null;
            }

            BitmapImage iconSource = null;

            if (element.Icon != null || element.ElementType != RibbonElementType.SmallToggleButton)
            {
                var icon = element.Icon != null ? new Icon(element.Icon) : Icon.Empty;
                iconSource = icon.GetSource();
            }

            var button = new ToggleButton()
            {
                Header = string.IsNullOrEmpty(element.Text) ? command.Text : element.Text,
                IsEnabled = command.CanExecute(parameter),
                Icon = iconSource,
                LargeIcon = iconSource,
                SizeDefinition = element.ElementType == RibbonElementType.SmallToggleButton ? "Middle" : "Large"
            };

            if (!string.IsNullOrEmpty(command.ToolTip))
            {
                button.ToolTip = command.ToolTip;
            }

            if (!string.IsNullOrEmpty(command.InputGestureText))
            {
                button.KeyTip = command.InputGestureText;
            }

            button.Click += delegate
            {
                var context = getContext();
                if (context == null)
                {
                    return;
                }

                if (command.CanExecute(context))
                {
                    command.Execute(context);
                }
            };

            if (!isContextual)
            {
                _toolbarButtons[button] = command;
            }

            return button;
        }
    }
}
