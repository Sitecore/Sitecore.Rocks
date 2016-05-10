// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.UI.LayoutDesigners.Extensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers
{
    public partial class DesignerControl
    {
        public DesignerControl([CanBeNull] object parameter)
        {
            InitializeComponent();

            Parameter = parameter;

            AppHost.Extensibility.ComposeParts(this);

            Loaded += ControlLoaded;
        }

        [CanBeNull]
        public object Parameter { get; }

        [NotNull, ImportMany(typeof(BaseDesigner))]
        protected IEnumerable<BaseDesigner> Designers { get; set; }

        public void HandleClosed()
        {
            foreach (var tabItem in Tabs.Children.OfType<Expander>())
            {
                var designer = tabItem.Content as IDesigner;
                if (designer != null)
                {
                    designer.Close();
                }
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            RenderHeader();
            RenderDesigners();
        }

        private void RenderDesigners()
        {
            Tabs.Children.Clear();

            foreach (var designer in Designers)
            {
                if (!designer.CanDesign(Parameter))
                {
                    continue;
                }

                var control = designer.GetDesigner(Parameter);
                if (control == null)
                {
                    continue;
                }

                var border = new Border
                {
                    Child = control,
                    Style = FindResource("SectionExpander") as Style
                };

                var expander = new Expander
                {
                    IsExpanded = AppHost.Settings.GetBool(@"AppBuilder\Properties", designer.Name, true),
                    Header = designer.Name,
                    Content = border,
                    Style = FindResource("ExpanderTriangle") as Style
                };

                expander.Expanded += SetExpanderState;
                expander.Collapsed += SetExpanderState;

                var designerControl = control as IDesigner;
                if (designerControl != null)
                {
                    designerControl.Activate();
                }

                Tabs.Children.Add(expander);
            }
        }

        private void RenderHeader()
        {
            var context = Parameter as RenderingContext;

            HeaderGrid.Visibility = context != null ? Visibility.Visible : Visibility.Collapsed;
            ResourcesExpander.Visibility = context != null ? Visibility.Visible : Visibility.Collapsed;

            if (context == null)
            {
                return;
            }

            var rendering = context.RenderingTreeViewItem.Rendering;

            IdTextBox.IsEnabled = rendering.DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, "Id", StringComparison.InvariantCultureIgnoreCase) == 0) != null;
            DataContext = rendering;

            IconImage.Source = rendering.Icon.Resize(IconSize.Size32x32).GetSource();
            RenderingTextBlock.Text = rendering.Name;

            RenderResources(rendering);
        }

        private void RenderResource([NotNull] string fileName, [NotNull] RoutedEventHandler click)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(click, nameof(click));

            var textBlock = new TextBlock();

            var inline = new Run
            {
                Text = fileName
            };

            var hyperlink = new Hyperlink(inline)
            {
                Style = FindResource("ResourceLink") as Style
            };

            hyperlink.Click += click;

            textBlock.Inlines.Add(hyperlink);

            ResourcesPanel.Children.Add(textBlock);
        }

        private void RenderResources([NotNull] RenderingItem rendering)
        {
            Debug.ArgumentNotNull(rendering, nameof(rendering));

            ResourcesExpander.IsExpanded = AppHost.Settings.GetBool(@"AppBuilder\Properties", "__Resources", false);

            RenderResource(rendering.Name + ".item", (sender, args) => AppHost.OpenContentEditor(new ItemVersionUri(rendering.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest)));

            var parameterTemplateId = rendering.ParameterTemplateId;
            if (!string.IsNullOrEmpty(parameterTemplateId))
            {
                RenderResource("Rendering Parameters.template", (sender, args) => AppHost.Windows.OpenTemplateDesigner(new ItemUri(rendering.ItemUri.DatabaseUri, new ItemId(Guid.Parse(parameterTemplateId)))));
            }

            var fileName = AppHost.Files.GetRenderingFileName(rendering, rendering.FilePath);
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                RenderResource(Path.GetFileName(fileName), (sender, args) => AppHost.Files.OpenFile(fileName));
            }

            var javascript = Path.ChangeExtension(fileName, ".js");
            if (!string.IsNullOrEmpty(javascript) && File.Exists(javascript))
            {
                RenderResource(Path.GetFileName(javascript), (sender, args) => AppHost.Files.OpenFile(javascript));
            }

            var stylesheet = Path.ChangeExtension(fileName, ".css");
            if (!string.IsNullOrEmpty(stylesheet) && File.Exists(stylesheet))
            {
                RenderResource(Path.GetFileName(stylesheet), (sender, args) => AppHost.Files.OpenFile(stylesheet));
            }
        }

        private void SetExpanderState([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var expander = sender as Expander;
            if (expander == null)
            {
                return;
            }

            var name = expander.Header as string;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            AppHost.Settings.SetBool(@"AppBuilder\Properties", name, expander.IsExpanded);
        }

        private void SetResourcesState([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.SetBool(@"AppBuilder\Properties", "__Resources", ResourcesExpander.IsExpanded);
        }
    }
}
