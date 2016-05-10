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

namespace Sitecore.Rocks.ContentEditors.Skins.Tabbed
{
    [Skin("Tabbed")]
    public partial class TabbedSkin : ISupportsReusableFieldControls, ISupportsPanels, ISupportsPanelRendering
    {
        private readonly Dictionary<Field, Border> borders = new Dictionary<Field, Border>();

        public TabbedSkin()
        {
            InitializeComponent();

            PanelHelper = new PanelHelper(DockPanelTabs, OuterDock, InnerDock, TabPanels);
        }

        public ContentEditor ContentEditor { get; set; }

        public ContentModel ContentModel { get; set; }

        [NotNull]
        public PanelHelper PanelHelper { get; set; }

        public Site Site { get; set; }

        public void Clear()
        {
            Tabs.Items.Clear();
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

            Tabs.Items.Clear();

            var result = RenderFieldControls();

            return result;
        }

        public void RenderPanels([NotNull] ContentModel contentModel, [NotNull] IEnumerable<IPanel> panels)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));
            Assert.ArgumentNotNull(panels, nameof(panels));

            PanelHelper.RenderPanels(this, contentModel, panels);
        }

        void ISupportsPanels.DockFill(string tabHeader, double priority, Control userControl)
        {
            Debug.ArgumentNotNull(tabHeader, nameof(tabHeader));
            Debug.ArgumentNotNull(userControl, nameof(userControl));

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
        private Control RenderCheckBox([NotNull] StackPanel section, [NotNull] Field field)
        {
            Debug.ArgumentNotNull(section, nameof(section));
            Debug.ArgumentNotNull(field, nameof(field));

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return null;
            }

            var control = fieldControl.GetControl() as ContentControl;
            if (control == null)
            {
                return null;
            }

            control.VerticalContentAlignment = VerticalAlignment.Center;

            var label = new Default.Label
            {
                ShowColon = false,
                ContentEditor = ContentEditor,
                Field = field,
                VerticalAlignment = VerticalAlignment.Center,
            };

            control.Content = label;

            var border = new Border
            {
                Margin = new Thickness(8, 12, 8, 12),
                Child = control
            };

            border.GotKeyboardFocus += (sender, args) => ContentEditor.UpdateContextualRibbon(field);

            section.Children.Add(border);

            borders[field] = border;

            return fieldControl.GetFocusableControl();
        }

        [CanBeNull]
        private Control RenderControl([NotNull] StackPanel section, [NotNull] Field field)
        {
            Debug.ArgumentNotNull(section, nameof(section));
            Debug.ArgumentNotNull(field, nameof(field));

            if (string.Compare(field.ActualFieldType, @"checkbox", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.ActualFieldType, @"tristate", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return RenderCheckBox(section, field);
            }

            var fieldControl = field.Control;
            if (fieldControl == null)
            {
                return null;
            }

            var label = new Default.Label
            {
                ContentEditor = ContentEditor,
                Field = field,
                Margin = new Thickness(8, 12, 8, 2)
            };

            section.Children.Add(label);

            var control = fieldControl.GetControl();

            var border = new Border
            {
                Margin = new Thickness(8, 0, 8, 12),
                Child = control
            };

            border.GotKeyboardFocus += (sender, args) => ContentEditor.UpdateContextualRibbon(field);

            section.Children.Add(border);

            borders[field] = border;

            return fieldControl.GetFocusableControl();
        }

        [CanBeNull]
        private Control RenderFieldControls()
        {
            if (ContentModel.IsSingle && ContentModel.FirstItem.Versions.Count == 0)
            {
                var tabItem = new TabItem
                {
                    Header = Rocks.Resources.TabbedSkin_RenderFields_Fields
                };

                Tabs.Items.Add(tabItem);

                tabItem.Content = new NoVersionsPane();

                return null;
            }

            Control result = null;
            string sectionName = null;
            StackPanel sectionControl = null;

            foreach (var field in ContentModel.Fields)
            {
                if (!field.IsVisible)
                {
                    continue;
                }

                if (field.Section.Name != sectionName)
                {
                    sectionName = field.Section.Name;

                    sectionControl = new StackPanel();

                    var scrollViewer = new ScrollViewer();

                    var tabItem = new TabItem
                    {
                        Header = sectionName,

                        // Height = 32
                    };

                    scrollViewer.Content = sectionControl;
                    tabItem.Content = scrollViewer;
                    Tabs.Items.Add(tabItem);
                }

                var control = RenderControl(sectionControl, field);
                if (result == null && control != null)
                {
                    result = control;
                }
            }

            if (sectionName == null)
            {
                RenderNoFields();
            }

            return result;
        }

        private void RenderNoFields()
        {
            var tabItem = new TabItem
            {
                Header = Rocks.Resources.TabbedSkin_RenderNoFields_Fields
            };

            Tabs.Items.Add(tabItem);

            var pane = new NoFieldsPane(ContentEditor);

            tabItem.Content = pane;
        }

        private void TabSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            PanelHelper.TabSelectionChanged(sender, e);
        }
    }
}
