// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateDesigner
{
    public class TemplateDesignerAdorner : Adorner
    {
        public TemplateDesignerAdorner([NotNull] UIElement adornedElement) : base(adornedElement)
        {
            Assert.ArgumentNotNull(adornedElement, nameof(adornedElement));

            IsHitTestVisible = false;
            Y = -1;
        }

        public int Y { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Debug.ArgumentNotNull(drawingContext, nameof(drawingContext));

            if (Y == -1)
            {
                return;
            }

            var highlightBrush = SystemColors.HighlightBrush;
            var brush = new SolidColorBrush(Color.FromRgb(highlightBrush.Color.R, highlightBrush.Color.G, highlightBrush.Color.B));
            var pen = new Pen(brush, 2);

            drawingContext.DrawLine(pen, new Point(13, Y), new Point(200, Y));

            var start = new Point(5, Y - 5);
            var segments = new[]
            {
                new LineSegment(new Point(10, Y), true),
                new LineSegment(new Point(5, Y + 5), true)
            };
            var figure = new PathFigure(start, segments, true);
            var geo = new PathGeometry(new[]
            {
                figure
            });
            drawingContext.DrawGeometry(highlightBrush, null, geo);
        }
    }
}
