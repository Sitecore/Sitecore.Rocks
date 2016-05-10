// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Overlays
{
    public class OverlayConnector
    {
        private readonly ConnectorAdorner adorner;

        [NotNull]
        private readonly AdornerLayer adornerLayer;

        public OverlayConnector([NotNull] OverlayCanvas overlayCanvas, Point start)
        {
            Assert.ArgumentNotNull(overlayCanvas, nameof(overlayCanvas));

            OverlayCanvas = overlayCanvas;
            Start = start;
            End = start;

            adorner = new ConnectorAdorner(this, OverlayCanvas);

            adornerLayer = AdornerLayer.GetAdornerLayer(OverlayCanvas);
            adornerLayer.Add(adorner);
        }

        public Point End { get; private set; }

        [NotNull]
        public OverlayCanvas OverlayCanvas { get; }

        public Point Start { get; }

        public void Remove()
        {
            adornerLayer.Remove(adorner);
        }

        public void SetPosition(Point position)
        {
            End = position;
            adornerLayer.Update();
        }

        public void UpdatePosition()
        {
            var position = Mouse.GetPosition(OverlayCanvas);
            if (Start.X != position.X)
            {
                SetPosition(position);
            }
        }

        public class ConnectorAdorner : Adorner
        {
            private readonly Pen pen;

            public ConnectorAdorner([NotNull] OverlayConnector overlayConnector, [NotNull] UIElement adornedElement) : base(adornedElement)
            {
                Assert.ArgumentNotNull(overlayConnector, nameof(overlayConnector));
                Assert.ArgumentNotNull(adornedElement, nameof(adornedElement));

                IsHitTestVisible = false;
                OverlayConnector = overlayConnector;
                pen = new Pen(SystemColors.HighlightBrush, 2);
            }

            [NotNull]
            public OverlayConnector OverlayConnector { get; }

            protected override void OnRender(DrawingContext drawingContext)
            {
                Debug.ArgumentNotNull(drawingContext, nameof(drawingContext));

                drawingContext.DrawLine(pen, OverlayConnector.Start, OverlayConnector.End);
            }
        }
    }
}
