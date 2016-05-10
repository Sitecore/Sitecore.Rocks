// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public class ControlDragAdorner
    {
        private LineAdorner _adorner;

        public ControlDragAdorner([NotNull] FrameworkElement control, ControlDragAdornerPosition allowedPositions)
        {
            Assert.ArgumentNotNull(control, nameof(control));

            Control = control;
            AllowedPositions = allowedPositions;

            control.DragEnter += ControlDragEnter;
            control.DragLeave += ControlDragLeave;
            control.DragOver += ControlDragMove;
            control.Drop += ControlDrop;
        }

        public ControlDragAdornerPosition AllowedPositions { get; set; }

        public ControlDragAdornerPosition LastPosition { get; set; }

        [NotNull]
        protected FrameworkElement Control { get; set; }

        public void Dropped()
        {
            if (_adorner == null)
            {
                return;
            }

            var adornerLayer = AdornerLayer.GetAdornerLayer(Control);
            adornerLayer.Remove(_adorner);

            _adorner = null;
        }

        public ControlDragAdornerPosition GetHitTest([NotNull] DragEventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));

            var pos = e.GetPosition(Control);

            var top = 5;
            var bottom = Control.DesiredSize.Height - 5;

            if ((AllowedPositions & ControlDragAdornerPosition.Over) != ControlDragAdornerPosition.Over)
            {
                top = (int)Control.DesiredSize.Height / 2;
                bottom = top;
            }

            var position = ControlDragAdornerPosition.None;
            if (pos.Y < top && (AllowedPositions & ControlDragAdornerPosition.Top) == ControlDragAdornerPosition.Top)
            {
                position = ControlDragAdornerPosition.Top;
            }
            else if (pos.Y >= bottom && (AllowedPositions & ControlDragAdornerPosition.Bottom) == ControlDragAdornerPosition.Bottom)
            {
                position = ControlDragAdornerPosition.Bottom;
            }
            else if ((AllowedPositions & ControlDragAdornerPosition.Over) == ControlDragAdornerPosition.Over)
            {
                position = ControlDragAdornerPosition.Over;
            }

            return position;
        }

        public void Remove()
        {
            Dropped();

            Control.DragEnter -= ControlDragEnter;
            Control.DragLeave -= ControlDragLeave;
            Control.DragOver -= ControlDragMove;
            Control.Drop -= ControlDrop;
        }

        private void ControlDragEnter([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!Control.AllowDrop)
            {
                return;
            }

            _adorner = new LineAdorner(Control);
            _adorner.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            var adornerLayer = AdornerLayer.GetAdornerLayer(Control);

            adornerLayer.Add(_adorner);

            SetPosition(e);

            e.Handled = true;
        }

        private void ControlDragLeave([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Dropped();

            e.Handled = true;
        }

        private void ControlDragMove([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (_adorner != null)
            {
                SetPosition(e);
            }
        }

        private void ControlDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Dropped();
        }

        private void SetPosition([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var position = GetHitTest(e);

            if (position != _adorner.Position)
            {
                Update(position);
            }
        }

        private void Update(ControlDragAdornerPosition position)
        {
            LastPosition = position;

            _adorner.Position = position;

            var adornerLayer = AdornerLayer.GetAdornerLayer(Control);
            adornerLayer.Update();
        }

        public class LineAdorner : Adorner
        {
            public LineAdorner([NotNull] UIElement adornedElement) : base(adornedElement)
            {
                Assert.ArgumentNotNull(adornedElement, nameof(adornedElement));

                IsHitTestVisible = false;
            }

            public ControlDragAdornerPosition Position { get; set; }

            protected override void OnRender(DrawingContext drawingContext)
            {
                Debug.ArgumentNotNull(drawingContext, nameof(drawingContext));

                if (Position == ControlDragAdornerPosition.None)
                {
                    return;
                }

                var rect = new Rect(AdornedElement.DesiredSize);

                var highlightBrush = SystemColors.HighlightBrush;

                if (Position == ControlDragAdornerPosition.Over)
                {
                    rect.Inflate(2, 1);
                    var overBrush = new SolidColorBrush(Color.FromArgb(80, highlightBrush.Color.R, highlightBrush.Color.G, highlightBrush.Color.B));
                    var overPen = new Pen(new SolidColorBrush(Color.FromRgb(highlightBrush.Color.R, highlightBrush.Color.G, highlightBrush.Color.B)), 1);
                    drawingContext.DrawRectangle(overBrush, overPen, rect);
                    return;
                }

                var brush = new SolidColorBrush(Color.FromRgb(highlightBrush.Color.R, highlightBrush.Color.G, highlightBrush.Color.B));
                var pen = new Pen(brush, 2);
                var p = Position != ControlDragAdornerPosition.Bottom ? rect.TopLeft : rect.BottomLeft;

                drawingContext.DrawLine(pen, new Point(p.X + 8, p.Y), new Point(p.X + 150, p.Y));

                var start = new Point(p.X + 1, p.Y - 5);
                var segments = new[]
                {
                    new LineSegment(new Point(p.X + 5, p.Y), true),
                    new LineSegment(new Point(p.X + 1, p.Y + 5), true)
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
}
