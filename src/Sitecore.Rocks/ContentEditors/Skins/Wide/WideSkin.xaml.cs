// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Panels;
using Sitecore.Rocks.ContentEditors.Panes;
using Sitecore.Rocks.ContentEditors.Validators;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentEditors.Skins.Wide
{
    [Skin("Wide")]
    public partial class WideSkin : ISupportsReusableFieldControls, ISupportsPanels, ISupportsPanelRendering
    {
        private readonly Dictionary<Field, Border> borders = new Dictionary<Field, Border>();

        public WideSkin()
        {
            InitializeComponent();

            PanelHelper = new PanelHelper(Tabs, OuterDock, InnerDock, TabPanels);
        }

        public ContentEditor ContentEditor { get; set; }

        public ContentModel ContentModel { get; set; }

        public Site Site { get; set; }

        protected PanelHelper PanelHelper { get; set; }

        public void Clear()
        {
            Editor.Children.Clear();
        }

        public Control GetControl()
        {
            return this;
        }

        public ValidatorBar GetValidatorBar()
        {
            return ValidateBar;
        }

        public bool RemoveFieldControl(Field field)
        {
            Assert.ArgumentNotNull(field, nameof(field));

            Border border;
            if (!borders.TryGetValue(field, out border))
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

            var result = RenderFieldControls();

            return result;
        }

        public void RenderPanels(ContentModel contentModel, IEnumerable<IPanel> panels)
        {
            PanelHelper.RenderPanels(this, contentModel, panels);
        }

        void ISupportsPanels.DockFill(string tabHeader, double priority, Control userControl)
        {
            Assert.ArgumentNotNull(tabHeader, nameof(tabHeader));
            Assert.ArgumentNotNull(userControl, nameof(userControl));

            PanelHelper.DockFill(tabHeader, priority, userControl);
        }

        void ISupportsPanels.DockInner(Control userControl, Dock dockPosition)
        {
            Debug.ArgumentNotNull(userControl, nameof(userControl));

            PanelHelper.DockInner(userControl, dockPosition);
        }

        void ISupportsPanels.DockOuter(Control userControl, Dock dockPosition)
        {
            Debug.ArgumentNotNull(userControl, nameof(userControl));

            PanelHelper.DockOuter(userControl, dockPosition);
        }

        [CanBeNull]
        private Control RenderCheckBox([NotNull] Grid grid, [NotNull] Field field)
        {
            Debug.ArgumentNotNull(grid, nameof(grid));
            Debug.ArgumentNotNull(field, nameof(field));

            if (field.Control == null)
            {
                return null;
            }

            var control = field.Control.GetControl() as ContentControl;
            if (control == null)
            {
                return null;
            }

            var label = new Default.Label
            {
                ShowColon = false,
                ContentEditor = ContentEditor,
                Field = field,
            };

            control.Content = label;

            var border = new Border
            {
                Margin = new Thickness(8, 12, 8, 12),
                Child = control
            };

            border.GotKeyboardFocus += (sender, args) => ContentEditor.UpdateContextualRibbon(field);

            grid.Children.Add(border);

            border.SetValue(Grid.ColumnProperty, 1);
            border.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);

            borders[field] = border;

            return field.Control.GetFocusableControl();
        }

        [CanBeNull]
        private Control RenderControl([NotNull] Grid grid, [NotNull] Field field)
        {
            Assert.ArgumentNotNull(grid, nameof(grid));
            Assert.ArgumentNotNull(field, nameof(field));

            if (string.Compare(field.ActualFieldType, @"checkbox", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.ActualFieldType, @"tristate", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return RenderCheckBox(grid, field);
            }

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return null;
            }

            RenderLabel(grid, field);

            var control = fieldControl.GetControl();

            /*
      if (!(control is TextField))
      {
        control.VerticalContentAlignment = VerticalAlignment.Top;
      }
      */

            var border = new Border
            {
                Margin = new Thickness(8, 12, 8, 12),
                Child = control
            };

            border.GotKeyboardFocus += (sender, args) => ContentEditor.UpdateContextualRibbon(field);

            grid.Children.Add(border);

            border.SetValue(Grid.ColumnProperty, 1);
            border.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);

            borders[field] = border;

            return fieldControl.GetFocusableControl();
        }

        [CanBeNull]
        private Control RenderFieldControls()
        {
            if (ContentModel.IsSingle && ContentModel.FirstItem.Versions.Count == 0)
            {
                Panels.Child = new NoVersionsPane();
                Panels.Visibility = Visibility.Visible;
                Editor.Visibility = Visibility.Collapsed;
                return null;
            }

            Control result = null;
            string sectionName = null;
            Grid grid = null;

            foreach (var field in ContentModel.Fields)
            {
                if (!field.IsVisible)
                {
                    continue;
                }

                if (field.Section.Name != sectionName)
                {
                    sectionName = field.Section.Name;
                    grid = RenderSection(field.Section.ItemUri, field.Section.Name, field.Section.ExpandedByDefault);
                }

                grid.RowDefinitions.Add(new RowDefinition());

                var control = RenderControl(grid, field);
                if (result == null && control != null)
                {
                    result = control;
                }
            }

            if (sectionName == null)
            {
                Panels.Child = new NoFieldsPane(ContentEditor);
                Panels.Visibility = Visibility.Visible;
                Editor.Visibility = Visibility.Collapsed;
                return null;
            }

            Panels.Visibility = Visibility.Collapsed;
            Editor.Visibility = Visibility.Visible;
            return result;
        }

        private void RenderLabel([NotNull] Grid grid, [NotNull] Field field)
        {
            Debug.ArgumentNotNull(grid, nameof(grid));
            Assert.ArgumentNotNull(field, nameof(field));

            var label = new Default.Label
            {
                ContentEditor = ContentEditor,
                Field = field,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 14, 0, 12)
            };

            grid.Children.Add(label);

            label.SetValue(Grid.ColumnProperty, 0);
            label.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
        }

        [NotNull]
        private Grid RenderSection([NotNull] ItemUri sectionItemUri, [NotNull] string sectionName, bool expandedByDefault)
        {
            Assert.ArgumentNotNull(sectionName, nameof(sectionName));

            var result = new Default.Section
            {
                ItemUri = sectionItemUri,
                ExpandedByDefault = expandedByDefault,
                Text = sectionName,
                ContentEditor = ContentEditor,
            };

            Editor.Children.Add(result);

            var grid = new Grid
            {
                // Margin = new Thickness(0, 8, 0, 0)
            };

            result.Children.Add(grid);

            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto,
                MinWidth = 150
            });

            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

            return grid;
        }

        private void TabSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            PanelHelper.TabSelectionChanged(sender, e);
        }
    }
}
