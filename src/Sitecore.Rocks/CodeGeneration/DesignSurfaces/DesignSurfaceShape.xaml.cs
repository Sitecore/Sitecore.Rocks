// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces
{
    public partial class DesignSurfaceShape : IShape
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(@"IsSelected", typeof(bool), typeof(DesignSurfaceShape), new PropertyMetadata(false));

        private readonly IShapeContent shapeContent;

        public DesignSurfaceShape([NotNull] IShapeContent shapeContent)
        {
            Assert.ArgumentNotNull(shapeContent, nameof(shapeContent));

            InitializeComponent();

            this.shapeContent = shapeContent;

            HeaderText.Text = shapeContent.Header;
            Presenter.Content = this.shapeContent;
        }

        [NotNull]
        public string Header
        {
            get { return HeaderText.Text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                HeaderText.Text = value;
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }

            set { SetValue(IsSelectedProperty, value); }
        }

        public IShapeContent ShapeContent
        {
            get { return shapeContent; }
        }

        public Point GetPosition()
        {
            return new Point(Margin.Left, Margin.Top);
        }

        public event MouseButtonEventHandler HeaderClick;

        public void Load(XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var left = element.GetAttributeDouble("designerX", 0);
            var top = element.GetAttributeDouble("designerY", 0);

            SetPosition(new Point(left, top));

            ShapeContent.Load(element);

            HeaderText.Text = ShapeContent.Header;
        }

        public void Save(XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            output.WriteStartElement("shape");

            var pos = GetPosition();
            output.WriteAttributeString("designerX", ((int)pos.X).ToString(CultureInfo.CurrentCulture));
            output.WriteAttributeString("designerY", ((int)pos.Y).ToString(CultureInfo.CurrentCulture));

            ShapeContent.Save(output);

            output.WriteEndElement();
        }

        public void SetPosition(Point position)
        {
            Margin = new Thickness(position.X, position.Y, 0, 0);
        }

        private void ClickHeader([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            HeaderGrid.Focus();

            var headerClick = HeaderClick;
            if (headerClick != null)
            {
                Dispatcher.BeginInvoke(new Action(() => headerClick(sender, e)));
            }
        }
    }
}
