// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.CodeGeneration.DesignSurfaces;
using Sitecore.Rocks.CodeGeneration.TemplateClasses.Models;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.CodeGeneration.TemplateClasses
{
    public partial class TemplateShapeContent : IShapeContent, IReloadable, IItemUri
    {
        public TemplateShapeContent([NotNull] IShapeCreator shapeCreator, [NotNull] ItemUri templateUri, [NotNull] string header)
        {
            Assert.ArgumentNotNull(shapeCreator, nameof(shapeCreator));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(header, nameof(header));

            InitializeComponent();

            ShapeCreator = shapeCreator;
            Header = header;
            Template = shapeCreator.CreateTemplate(ShapeCreator, templateUri);

            Loaded += ControlLoaded;
        }

        public TemplateShapeContent([NotNull] IShapeCreator shapeCreator)
        {
            InitializeComponent();

            ShapeCreator = shapeCreator;
            Template = shapeCreator.CreateTemplate(ShapeCreator, ItemUri.Empty);
        }

        [NotNull]
        public string Header { get; private set; }

        public ItemUri ItemUri
        {
            get { return Template.TemplateUri; }
        }

        public new Template Template { get; }

        protected IShapeCreator ShapeCreator { get; set; }

        public void Initialize([NotNull] IShape shape)
        {
            Assert.ArgumentNotNull(shape, nameof(shape));

            shape.HeaderClick += FocusHeader;
        }

        public void Load(XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var templateElement = element.Element("template");

            Template.Load(templateElement);

            Header = Template.Name;

            RenderTemplate();

            Loading.HideLoading(TemplateListView);
        }

        public void Reload()
        {
            Loading.ShowLoading(TemplateListView);

            var templateUri = Template.TemplateUri;

            GetValueCompleted<XDocument> completed = delegate(XDocument doc)
            {
                Debug.ArgumentNotNull(doc, nameof(doc));

                Loading.HideLoading(TemplateListView);

                var templateElement = doc.XPathSelectElement(@"/template");
                if (templateElement != null)
                {
                    Template.Parse(templateElement);
                }

                RenderTemplate();
            };

            templateUri.Site.DataService.GetTemplateXml(templateUri, false, completed);
        }

        public void Save(XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            Template.Save(output);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            Reload();
        }

        private void FocusHeader([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var objects = new List<object>
            {
                Template
            };

            TrackSelection(objects);
        }

        private void GotFocusTrackSelection([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TrackSelection(TemplateListView.SelectedItems.Cast<object>().ToList());
        }

        private void GotKeyboardFocusTrackSelection([NotNull] object sender, [NotNull] KeyboardFocusChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TrackSelection(TemplateListView.SelectedItems.Cast<object>().ToList());
        }

        private void LostFocusTrackSelection([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TrackSelection(null);
        }

        private void RenderTemplate()
        {
            var list = new List<object>();

            foreach (var templateSection in Template.TemplateSections)
            {
                list.Add(templateSection);

                list.AddRange(templateSection.Fields);
            }

            TemplateListView.ItemsSource = null;
            TemplateListView.ItemsSource = list;
        }

        private void SelectedChangedTrackSelection([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TrackSelection(TemplateListView.SelectedItems.Cast<object>().ToList());
        }

        private void TrackSelection([CanBeNull] IEnumerable<object> objects)
        {
            var configurator = this.GetAncestorOrSelf<CodeGenerationConfigurator>();
            if (configurator != null)
            {
                Shell.TrackSelection.SelectObjects(configurator.Pane, objects);
            }
        }
    }
}
