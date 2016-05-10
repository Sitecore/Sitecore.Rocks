// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Bindings;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.DropDowns;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.ParameterDesigners
{
    public partial class ParameterDesignerControl : IDesigner
    {
        private bool activated;

        public ParameterDesignerControl([NotNull] PageModel pageModel, [NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(pageModel, nameof(pageModel));
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            InitializeComponent();

            PageModel = pageModel;
            Rendering = rendering;
        }

        [NotNull]
        public PageModel PageModel { get; }

        [NotNull]
        protected RenderingItem Rendering { get; }

        public void Activate()
        {
            if (activated)
            {
                return;
            }

            activated = true;

            RenderForm();
        }

        public void Close()
        {
        }

        private void Add([NotNull] DynamicProperty dynamicProperty, [NotNull] string label, [NotNull] UIElement input)
        {
            Debug.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));
            Debug.ArgumentNotNull(label, nameof(label));
            Debug.ArgumentNotNull(input, nameof(input));

            var text = label;
            var textBlock = new TextBlock
            {
                Text = text,
                Style = TryFindResource("ParameterLabel") as Style,
                ToolTip = text
            };

            var bindMode = (Properties.BindingMode)dynamicProperty.Attributes["bindmode"];
            if (bindMode == Properties.BindingMode.ReadWrite || bindMode == Properties.BindingMode.Write || string.Compare(dynamicProperty.Category, @"Data Bindings", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                Form.Children.Add(new BindingAnchor(PageModel, Rendering, dynamicProperty));
            }
            else
            {
                Form.Children.Add(new Border());
            }

            Form.Children.Add(textBlock);
            Form.Children.Add(input);

            var hasEditor = false;

            var propertyDescriptor = TypeDescriptor.GetProperties(Rendering, false).OfType<PropertyDescriptor>().FirstOrDefault(p => p.Name == dynamicProperty.Name);
            if (propertyDescriptor != null)
            {
                foreach (var attribute in propertyDescriptor.Attributes.OfType<EditorAttribute>())
                {
                    var button = new Button
                    {
                        Margin = new Thickness(2, 0, 0, 0),
                        Content = "...",
                        Height = 21
                    };

                    var a = attribute;
                    button.Click += (sender, args) => OpenEditor(dynamicProperty, propertyDescriptor, a.EditorTypeName);

                    Form.Children.Add(button);
                    hasEditor = true;
                    break;
                }
            }

            if (!hasEditor)
            {
                Form.Children.Add(new Border());
            }
        }

        private void AddCheckBox([NotNull] string label, [NotNull] DynamicProperty dynamicProperty)
        {
            Debug.ArgumentNotNull(label, nameof(label));
            Debug.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));

            var checkBox = new CheckBox
            {
                Margin = new Thickness(0, 2, 0, 2),
                VerticalAlignment = VerticalAlignment.Center
            };

            var binding = new Binding(dynamicProperty.Name)
            {
                Source = Rendering,
                Mode = BindingMode.TwoWay
            };

            checkBox.SetBinding(ToggleButton.IsCheckedProperty, binding);

            Add(dynamicProperty, label, checkBox);
        }

        private void AddComboBox([NotNull] string label, [NotNull] DynamicProperty dynamicProperty)
        {
            Debug.ArgumentNotNull(label, nameof(label));
            Debug.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));

            var propertyDescriptor = TypeDescriptor.GetProperties(Rendering, false).OfType<PropertyDescriptor>().FirstOrDefault(p => p.Name == dynamicProperty.Name);
            if (propertyDescriptor == null)
            {
                return;
            }

            var typeConverter = dynamicProperty.Converter;
            if (typeConverter == null)
            {
                return;
            }

            var values = typeConverter.GetStandardValues(new TypeDescriptorContext(propertyDescriptor));
            if (values == null)
            {
                return;
            }

            var comboBox = new ComboBox
            {
                IsEditable = true,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 2)
            };

            foreach (var value in values)
            {
                comboBox.Items.Add(value.ToString());
            }

            var binding = new Binding(dynamicProperty.Name)
            {
                Source = Rendering,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            if (dynamicProperty.Type == typeof(DropDownValue))
            {
                binding.Converter = new DropDownConverter();
            }

            comboBox.SetBinding(ComboBox.TextProperty, binding);

            Add(dynamicProperty, label, comboBox);
        }

        private void AddSection([NotNull] string text)
        {
            Debug.ArgumentNotNull(text, nameof(text));

            var textBlock = new TextBlock
            {
                Style = TryFindResource("ParameterSection") as Style,
                Text = text
            };

            Form.Children.Add(textBlock);
        }

        private void AddTextBox([NotNull] string label, [NotNull] DynamicProperty dynamicProperty)
        {
            Debug.ArgumentNotNull(label, nameof(label));
            Debug.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));

            var textBox = new TextBox
            {
                Margin = new Thickness(0, 2, 0, 2),
                VerticalAlignment = VerticalAlignment.Center
            };

            var binding = new Binding(dynamicProperty.Name)
            {
                Source = Rendering,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            textBox.SetBinding(TextBox.TextProperty, binding);

            Add(dynamicProperty, label, textBox);
        }

        private void OpenEditor([NotNull] DynamicProperty dynamicProperty, [NotNull] PropertyDescriptor propertyDescriptor, [NotNull] string editorTypeName)
        {
            Debug.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));
            Debug.ArgumentNotNull(propertyDescriptor, nameof(propertyDescriptor));
            Debug.ArgumentNotNull(editorTypeName, nameof(editorTypeName));

            var type = Type.GetType(editorTypeName);
            if (type == null)
            {
                return;
            }

            var editor = Activator.CreateInstance(type) as UITypeEditor;
            if (editor == null)
            {
                return;
            }

            var value = dynamicProperty.Value;
            var context = new TypeDescriptorContext(propertyDescriptor, Rendering);
            IServiceProvider provider = null;

            // ReSharper disable once AssignNullToNotNullAttribute - this is OK
            dynamicProperty.Value = editor.EditValue(context, provider, value);
        }

        private void RenderForm()
        {
            string section = null;

            foreach (var dynamicProperty in Rendering.DynamicProperties.OrderBy(d => d.Category).ThenBy(d => d.Name))
            {
                if (dynamicProperty.Name == "Id")
                {
                    continue;
                }

                if (dynamicProperty.IsReadOnly)
                {
                    continue;
                }

                if (dynamicProperty.IsHidden)
                {
                    continue;
                }

                if (dynamicProperty.Category != section)
                {
                    section = dynamicProperty.Category;
                    AddSection(section);
                }

                var displayName = dynamicProperty.DisplayName;
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = dynamicProperty.Name;
                }

                if (dynamicProperty.Type == typeof(bool?))
                {
                    AddCheckBox(displayName, dynamicProperty);
                }
                else if (dynamicProperty.Converter != null && dynamicProperty.Converter.GetStandardValuesSupported())
                {
                    AddComboBox(displayName, dynamicProperty);
                }
                else
                {
                    AddTextBox(displayName, dynamicProperty);
                }
            }
        }
    }
}
