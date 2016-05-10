// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Panels;
using Sitecore.Rocks.ContentEditors.Panes;
using Sitecore.Rocks.ContentEditors.Validators;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Skins.DbBrowser
{
    [Skin("DB Browser")]
    public partial class DbBrowserSkin : ISupportsReusableFieldControls, ISupportsPanels, ISupportsPanelRendering
    {
        private readonly Dictionary<Field, Border> borders = new Dictionary<Field, Border>();

        public DbBrowserSkin()
        {
            InitializeComponent();

            PanelHelper = new PanelHelper(Tabs, OuterDock, InnerDock, TabPanels);
        }

        public ContentEditor ContentEditor { get; set; }

        public ContentModel ContentModel { get; set; }

        [NotNull]
        public PanelHelper PanelHelper { get; set; }

        public int RowIndex { get; set; }

        public void Clear()
        {
            Editor.Children.Clear();
        }

        public void DockFill(string tabHeader, double priority, Control userControl)
        {
            Assert.ArgumentNotNull(tabHeader, nameof(tabHeader));
            Assert.ArgumentNotNull(userControl, nameof(userControl));

            PanelHelper.DockFill(tabHeader, priority, userControl);
        }

        public void DockInner(Control userControl, Dock dockPosition)
        {
            Assert.ArgumentNotNull(userControl, nameof(userControl));

            PanelHelper.DockInner(userControl, dockPosition);
        }

        public void DockOuter(Control userControl, Dock dockPosition)
        {
            Assert.ArgumentNotNull(userControl, nameof(userControl));

            PanelHelper.DockOuter(userControl, dockPosition);
        }

        public Control GetControl()
        {
            return this;
        }

        public ValidatorBar GetValidatorBar()
        {
            return ValidateBar;
        }

        public bool RemoveFieldControl(Field control)
        {
            Assert.ArgumentNotNull(control, nameof(control));

            Border border;
            if (!borders.TryGetValue(control, out border))
            {
                return false;
            }

            border.Child = null;

            return true;
        }

        public Control RenderFields()
        {
            InfoPane.Load(ContentEditor);

            Editor.Children.Clear();

            Editor.RowDefinitions.Clear();

            var result = RenderFieldControls();

            return result;
        }

        public void RenderPanels(ContentModel contentModel, IEnumerable<IPanel> panels)
        {
            PanelHelper.RenderPanels(this, contentModel, panels);
        }

        private void AddRow()
        {
            Editor.RowDefinitions.Add(new RowDefinition());

            RowIndex++;
        }

        [CanBeNull]
        private Control RenderControl([NotNull] Section section, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(section, nameof(section));
            Assert.ArgumentNotNull(field, nameof(field));

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return null;
            }

            var control = fieldControl.GetControl();

            control.VerticalAlignment = VerticalAlignment.Center;

            var border = new Border
            {
                Padding = new Thickness(4),
                Background = SystemColors.WindowBrush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = SystemColors.ControlDarkBrush,
                Child = control
            };

            border.GotKeyboardFocus += (sender, args) => ContentEditor.UpdateContextualRibbon(field);

            borders[field] = border;

            Editor.Children.Add(border);

            border.SetValue(Grid.RowProperty, RowIndex);
            border.SetValue(Grid.ColumnProperty, 1);

            return fieldControl.GetFocusableControl();
        }

        [CanBeNull]
        private Control RenderFieldControls()
        {
            if (ContentModel.IsSingle && ContentModel.FirstItem.Versions.Count == 0)
            {
                Editor.Children.Add(new NoVersionsPane());
                return null;
            }

            Control result = null;
            string sectionName = null;
            Section sectionControl = null;

            foreach (var field in ContentModel.Fields)
            {
                if (!field.IsVisible)
                {
                    continue;
                }

                if (field.Section.Name != sectionName)
                {
                    sectionName = field.Section.Name;
                    sectionControl = RenderSection(field.Section.Name);

                    AddRow();
                }

                RenderLabel(sectionControl, field);

                var control = RenderControl(sectionControl, field);
                if (result == null && control != null)
                {
                    result = control;
                }

                AddRow();
            }

            if (sectionName == null)
            {
                RenderNoFields();
            }

            return result;
        }

        private void RenderLabel([NotNull] Section section, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(section, nameof(section));
            Assert.ArgumentNotNull(field, nameof(field));

            var label = new Default.Label
            {
                ContentEditor = ContentEditor,
                Field = field,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Top,
                Padding = new Thickness(0, 4, 8, 0),
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = SystemColors.ControlDarkBrush,
            };

            label.HelpTextBlock.Visibility = Visibility.Collapsed;

            Editor.Children.Add(label);

            label.SetValue(Grid.RowProperty, RowIndex);
        }

        private void RenderNoFields()
        {
            var pane = new NoFieldsPane(ContentEditor);

            pane.SetValue(Grid.ColumnSpanProperty, 2);

            Editor.Children.Add(pane);
        }

        [NotNull]
        private Section RenderSection([NotNull] string sectionName)
        {
            Assert.ArgumentNotNull(sectionName, nameof(sectionName));

            var result = new Section
            {
                Text = sectionName
            };

            Editor.Children.Add(result);

            result.SetValue(Grid.RowProperty, RowIndex);
            result.SetValue(Grid.ColumnSpanProperty, 2);

            return result;
        }

        private void TabSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            PanelHelper.TabSelectionChanged(sender, e);
        }
    }
}
