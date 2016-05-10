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

namespace Sitecore.Rocks.ContentEditors.Skins.Default
{
    [Skin("Default")]
    public partial class DefaultSkin : ISupportsReusableFieldControls, ISupportsPanels, ISupportsPanelRendering
    {
        [NotNull]
        private readonly Dictionary<Field, Border> borders = new Dictionary<Field, Border>();

        public DefaultSkin()
        {
            InitializeComponent();

            Loaded += DefaultSkinLoaded;

            PanelHelper = new PanelHelper(Tabs, OuterDock, InnerDock, TabPanels);
        }

        public ContentEditor ContentEditor { get; set; }

        public ContentModel ContentModel { get; set; }

        [NotNull]
        public PanelHelper PanelHelper { get; set; }

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

        public void RenderPanels([NotNull] ContentModel contentModel, [NotNull] IEnumerable<IPanel> panels)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));
            Assert.ArgumentNotNull(panels, nameof(panels));

            PanelHelper.RenderPanels(this, contentModel, panels);
        }

        private void DefaultSkinLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= DefaultSkinLoaded;
            Editor.Focus();
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
        private Control RenderCheckBox([NotNull] Section section, [NotNull] Field field)
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

            var label = new Label
            {
                ShowColon = false,
                ContentEditor = ContentEditor,
                Field = field
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
        private Control RenderControl([NotNull] Section section, [NotNull] Field field)
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

            var label = new Label
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
                Panels.Child = new NoVersionsPane();
                Panels.Visibility = Visibility.Visible;
                Editor.Visibility = Visibility.Collapsed;
                return null;
            }

            string sectionName = null;
            Section sectionControl = null;
            Control result = null;

            foreach (var field in ContentModel.Fields)
            {
                if (!field.IsVisible)
                {
                    continue;
                }

                if (field.Section.Name != sectionName)
                {
                    sectionName = field.Section.Name;
                    sectionControl = RenderSection(field.Section.ItemUri, field.Section.Name, field.Section.Icon, field.Section.ExpandedByDefault);
                }

                var control = RenderControl(sectionControl, field);
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

        [NotNull]
        private Section RenderSection([NotNull] ItemUri sectionItemUri, [NotNull] string sectionName, [NotNull] Icon sectionIcon, bool expandedByDefault)
        {
            Debug.ArgumentNotNull(sectionItemUri, nameof(sectionItemUri));
            Debug.ArgumentNotNull(sectionName, nameof(sectionName));
            Debug.ArgumentNotNull(sectionIcon, nameof(sectionIcon));

            var result = new Section
            {
                ItemUri = sectionItemUri,
                Text = sectionName,
                Icon = sectionIcon,
                ContentEditor = ContentEditor,
                ExpandedByDefault = expandedByDefault
            };

            Editor.Children.Add(result);

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
