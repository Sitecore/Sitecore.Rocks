// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.Controls
{
    public class ListViewAdorner
    {
        public ListViewAdorner([NotNull] ListView listView)
        {
            Assert.ArgumentNotNull(listView, nameof(listView));

            ListView = listView;

            listView.DragEnter += DragEnter;
            listView.DragLeave += DragLeave;
            listView.DragOver += DragMove;
            listView.Drop += Drop;
        }

        [CanBeNull]
        protected LineAdorner Line { get; private set; }

        [NotNull]
        protected ListView ListView { get; set; }

        public void Drop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!ListView.AllowDrop)
            {
                return;
            }

            RemoveLine();
        }

        public int GetPosition([NotNull] DragEventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement == null)
            {
                return -1;
            }

            if (frameworkElement.GetAncestorOrSelf<GridViewHeaderRowPresenter>() != null)
            {
                return -1;
            }

            var offset = 0;

            var listViewItem = frameworkElement.GetAncestorOrSelf<ListViewItem>();
            if (listViewItem == null)
            {
                if (ListView.Items.Count == 0)
                {
                    return -1;
                }

                var item = ListView.Items[ListView.Items.Count - 1];
                listViewItem = ListView.FindChildren<ListViewItem>().FirstOrDefault(i => i.Content == item);
                if (listViewItem == null)
                {
                    return -1;
                }

                offset = (int)listViewItem.ActualHeight;
            }

            var pointToScreen = listViewItem.PointToScreen(new Point(0, offset));
            var pointFromScreen = ListView.PointFromScreen(pointToScreen);

            return (int)pointFromScreen.Y;
        }

        protected void Remove()
        {
            RemoveLine();

            ListView.DragEnter -= DragEnter;
            ListView.DragLeave -= DragLeave;
            ListView.DragOver -= DragMove;
            ListView.Drop -= Drop;
        }

        protected void RemoveLine()
        {
            if (Line == null)
            {
                return;
            }

            var layer = AdornerLayer.GetAdornerLayer(ListView);
            if (layer != null)
            {
                layer.Remove(Line);
            }

            Line = null;
        }

        private void DragEnter([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!ListView.AllowDrop)
            {
                return;
            }

            if (Line == null)
            {
                Line = new LineAdorner(ListView);
                Line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

                var adornerLayer = AdornerLayer.GetAdornerLayer(ListView);
                if (adornerLayer != null)
                {
                    adornerLayer.Add(Line);
                }
            }

            UpdatePosition(e);
        }

        private void DragLeave([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!ListView.AllowDrop)
            {
                return;
            }

            RemoveLine();
        }

        private void DragMove([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!ListView.AllowDrop)
            {
                return;
            }

            UpdatePosition(e);
        }

        private void UpdatePosition([NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            var line = Line;
            if (line == null)
            {
                return;
            }

            line.Y = GetPosition(e);

            line.Visibility = line.Y >= 0 ? Visibility.Visible : Visibility.Collapsed;

            var adornerLayer = AdornerLayer.GetAdornerLayer(ListView);
            if (adornerLayer != null)
            {
                adornerLayer.Update();
            }
        }

        public class LineAdorner : Adorner
        {
            public LineAdorner([NotNull] UIElement adornedElement) : base(adornedElement)
            {
                Assert.ArgumentNotNull(adornedElement, nameof(adornedElement));

                IsHitTestVisible = false;
            }

            public int Y { get; set; }

            protected override void OnRender(DrawingContext drawingContext)
            {
                Debug.ArgumentNotNull(drawingContext, nameof(drawingContext));

                var highlightBrush = SystemColors.HighlightBrush;
                var pen = new Pen(new SolidColorBrush(Color.FromRgb(highlightBrush.Color.R, highlightBrush.Color.G, highlightBrush.Color.B)), 2);

                var rect = new Rect(AdornedElement.DesiredSize);
                var p = new Point(8, rect.TopLeft.Y + Y);

                drawingContext.DrawLine(pen, p, new Point(150, p.Y));

                var start = new Point(1, p.Y - 5);
                var segments = new[]
                {
                    new LineSegment(new Point(5, p.Y), true),
                    new LineSegment(new Point(1, p.Y + 5), true)
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
