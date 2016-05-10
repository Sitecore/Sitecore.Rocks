// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.Dialogs.SelectFileDialogs;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.DataBindings;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.PlaceHolders;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs.RenderingEditors.Editors
{
    public class EditorBuilder
    {
        private readonly List<BindingExpressionBase> bindings = new List<BindingExpressionBase>();

        public EditorBuilder([NotNull] Grid grid, [NotNull] RenderingItem renderingItem)
        {
            Assert.ArgumentNotNull(grid, nameof(grid));
            Assert.ArgumentNotNull(renderingItem, nameof(renderingItem));

            Grid = grid;
            RenderingItem = renderingItem;
        }

        [NotNull]
        public Grid Grid { get; }

        [NotNull]
        public RenderingItem RenderingItem { get; }

        public void AddActionButton([NotNull] string text, [NotNull] RoutedEventHandler click)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(click, nameof(click));

            Grid.Children.Add(new Border());

            var button = new Button
            {
                Content = text,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = double.NaN
            };

            button.Click += click;

            Grid.Children.Add(button);
        }

        public void AddBindingProperty([NotNull] string text, [NotNull] string propertyName)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(propertyName, nameof(propertyName));

            AddLabel(text);

            var property = GetProperty(propertyName);
            var binding = new Binding(property.Name);

            var comboBox = new ComboBox();
            comboBox.SetBinding(Selector.SelectedItemProperty, binding);
            comboBox.VerticalAlignment = VerticalAlignment.Center;

            var renderingContainer = RenderingItem.RenderingContainer;
            if (renderingContainer != null)
            {
                var values = new List<string>();

                renderingContainer.GetDataBindingValues(RenderingItem, property, values);

                foreach (var value in values)
                {
                    if (property.Type == typeof(DataBinding))
                    {
                        comboBox.Items.Add(new DataBinding(value));
                    }
                    else
                    {
                        comboBox.Items.Add(value);
                    }
                }
            }

            comboBox.SelectedItem = property.Value;

            Grid.Children.Add(comboBox);

            bindings.Add(BindingOperations.GetBindingExpressionBase(comboBox, Selector.SelectedItemProperty));
        }

        public void AddButton([NotNull] string text, [NotNull] RoutedEventHandler click)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(click, nameof(click));

            var button = new Button
            {
                Content = text,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = double.NaN
            };

            button.SetValue(Grid.ColumnSpanProperty, 2);
            button.Click += click;

            Grid.Children.Add(button);
        }

        public void AddDataSource()
        {
            AddLabel("DataSource");

            var binding = new Binding("DataSource");

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, binding);
            textBox.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(textBox);
            textBox.SetValue(Grid.ColumnProperty, 0);

            var button = new Button
            {
                Content = "Browse",
                Width = 75,
                Height = 23,
                Margin = new Thickness(2, 0, 0, 0)
            };

            button.Click += delegate
            {
                var container = RenderingItem.RenderingContainer;
                if (container == null)
                {
                    return;
                }

                var dialog = new SelectItemDialog();
                dialog.Initialize("Data Source", RenderingItem.ItemUri.DatabaseUri, RenderingItem.DataSource ?? string.Empty);
                if (AppHost.Shell.ShowDialog(dialog) != true)
                {
                    return;
                }

                textBox.Text = dialog.SelectedItemUri.ItemId.ToString();
            };

            grid.Children.Add(button);
            button.SetValue(Grid.ColumnProperty, 1);

            Grid.Children.Add(grid);

            bindings.Add(BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty));
        }

        public void AddFileProperty([NotNull] string text, [NotNull] string propertyName)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(propertyName, nameof(propertyName));

            AddLabel(text);

            var property = GetProperty(propertyName);
            var binding = new Binding(property.Name);

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, binding);
            textBox.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(textBox);
            textBox.SetValue(Grid.ColumnProperty, 0);

            var button = new Button
            {
                Content = "Browse",
                Width = 75,
                Height = 23,
                Margin = new Thickness(2, 0, 0, 0)
            };

            button.Click += delegate
            {
                var dialog = new SelectFileDialog
                {
                    Site = RenderingItem.ItemUri.Site
                };

                if (dialog.ShowDialog())
                {
                    textBox.Text = dialog.SelectedFilePath.Replace("\\", "/");
                }
            };

            grid.Children.Add(button);
            button.SetValue(Grid.ColumnProperty, 1);

            Grid.Children.Add(grid);

            bindings.Add(BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty));
        }

        public void AddHeader([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            var textBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(4, 4, 4, 4),
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.Bold
            };

            textBlock.SetValue(Grid.ColumnSpanProperty, 2);

            Grid.Children.Add(textBlock);
        }

        public void AddId()
        {
            AddLabel("Control ID");

            var property = GetProperty("Id");
            var binding = new Binding(property.Name);

            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, binding);
            textBox.VerticalAlignment = VerticalAlignment.Center;

            Grid.Children.Add(textBox);

            bindings.Add(BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty));
        }

        public void AddIdProperty([NotNull] string text, [NotNull] string propertyName)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(propertyName, nameof(propertyName));

            AddLabel(text);

            var property = GetProperty(propertyName);
            var binding = new Binding(property.Name);

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, binding);
            textBox.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(textBox);
            textBox.SetValue(Grid.ColumnProperty, 0);

            var button = new Button
            {
                Content = "Browse",
                Width = 75,
                Height = 23,
                Margin = new Thickness(2, 0, 0, 0)
            };

            button.Click += delegate
            {
                var container = RenderingItem.RenderingContainer;
                if (container == null)
                {
                    return;
                }

                var dialog = new SelectItemDialog();
                dialog.Initialize("Browse", RenderingItem.ItemUri.DatabaseUri, textBox.Text ?? string.Empty);
                if (AppHost.Shell.ShowDialog(dialog) != true)
                {
                    return;
                }

                textBox.Text = dialog.SelectedItemUri.ItemId.ToString();
            };

            grid.Children.Add(button);
            button.SetValue(Grid.ColumnProperty, 1);

            Grid.Children.Add(grid);

            var path = new TextBlock
            {
                Text = " ",
                Margin = new Thickness(0, 0, 0, 4),
                Foreground = SystemColors.GrayTextBrush,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            Grid.Children.Add(new Border());
            Grid.Children.Add(path);

            textBox.TextChanged += delegate
            {
                path.Text = "[Updating path]";

                ExecuteCompleted completed = delegate(string response, ExecuteResult result)
                {
                    if (!DataService.HandleExecute(response, result, true))
                    {
                        path.Text = "[Item not found]";
                        return;
                    }

                    var element = response.ToXElement();
                    if (element == null)
                    {
                        path.Text = "[Item not found]";
                        return;
                    }

                    var itemHeader = ItemHeader.Parse(RenderingItem.ItemUri.DatabaseUri, element);

                    path.Text = string.IsNullOrEmpty(itemHeader.Path) ? " " : itemHeader.Path;
                };

                RenderingItem.ItemUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, textBox.Text, RenderingItem.ItemUri.DatabaseName.Name);
            };

            var selectedItem = property.Value;
            textBox.Text = (selectedItem ?? string.Empty).ToString();

            bindings.Add(BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty));
        }

        public void AddPlaceHolder()
        {
            AddLabel("Place Holder");

            var binding = new Binding("PlaceholderKey");

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, binding);
            textBox.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(textBox);
            textBox.SetValue(Grid.ColumnProperty, 0);

            var button = new Button
            {
                Content = "Browse",
                Width = 75,
                Height = 23,
                Margin = new Thickness(2, 0, 0, 0)
            };

            button.Click += delegate
            {
                var container = RenderingItem.RenderingContainer;
                if (container == null)
                {
                    return;
                }

                var key = RenderingItem.PlaceholderKey.ToString();

                var dialog = new PlaceholderDialog();
                dialog.Initialize(Resources.Browse, key, container);
                if (AppHost.Shell.ShowDialog(dialog) != true)
                {
                    return;
                }

                RenderingItem.PlaceholderKey = new PlaceHolderKey(dialog.SelectedValue);
                UpdateTarget();
            };

            grid.Children.Add(button);
            button.SetValue(Grid.ColumnProperty, 1);

            Grid.Children.Add(grid);

            bindings.Add(BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty));
        }

        public void AddStringProperty([NotNull] string text, [NotNull] string propertyName)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(propertyName, nameof(propertyName));

            AddLabel(text);

            var property = GetProperty(propertyName);
            var binding = new Binding(property.Name);

            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, binding);
            textBox.VerticalAlignment = VerticalAlignment.Center;

            Grid.Children.Add(textBox);

            bindings.Add(BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty));
        }

        public void AddText([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            var textBlock = new TextBlock
            {
                Text = text,
                Margin = new Thickness(4, 4, 4, 4),
                TextWrapping = TextWrapping.Wrap
            };

            textBlock.SetValue(Grid.ColumnSpanProperty, 2);

            Grid.Children.Add(textBlock);
        }

        public void AddVerticalSpace()
        {
            var border = new Border
            {
                Height = 16
            };

            border.SetValue(Grid.ColumnSpanProperty, 2);

            Grid.Children.Add(border);
        }

        public void Update()
        {
            foreach (var expression in bindings)
            {
                expression.UpdateSource();
            }
        }

        public void UpdateTarget()
        {
            foreach (var expression in bindings)
            {
                expression.UpdateTarget();
            }
        }

        private void AddLabel([NotNull] string text)
        {
            Debug.ArgumentNotNull(text, nameof(text));

            Grid.RowDefinitions.Add(new RowDefinition());

            var label = new Label
            {
                Content = text
            };

            Grid.Children.Add(label);
        }

        [NotNull]
        private DynamicProperty GetProperty([NotNull] string propertyName)
        {
            Debug.ArgumentNotNull(propertyName, nameof(propertyName));

            return RenderingItem.DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, propertyName, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}
