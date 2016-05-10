// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Overlays
{
    public partial class OverlayWindow : INotifyPropertyChanged
    {
        private string caption;

        private double horizontalOffset;

        private bool isOpen;

        private double left;

        private double top;

        private double verticalOffset;

        public OverlayWindow([NotNull] OverlayCanvas canvas, [NotNull] string caption)
        {
            Assert.ArgumentNotNull(canvas, nameof(canvas));
            Assert.ArgumentNotNull(caption, nameof(caption));

            InitializeComponent();

            DataContext = this;

            Canvas = canvas;
            Caption = caption;
        }

        [NotNull]
        public string Caption
        {
            get { return caption; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (caption == value)
                {
                    return;
                }

                caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        [CanBeNull]
        public UIElement Designer
        {
            get { return Border.Child; }

            set { Border.Child = value; }
        }

        public double HorizontalOffset
        {
            get { return horizontalOffset; }

            set
            {
                horizontalOffset = value;
                UpdatePosition();
            }
        }

        public bool IsOpen
        {
            get { return isOpen; }

            set
            {
                if (isOpen == value)
                {
                    return;
                }

                isOpen = value;

                if (isOpen)
                {
                    Canvas.Add(this);
                }
                else
                {
                    Canvas.Remove(this);
                    RaiseClosed();
                }
            }
        }

        public double Left
        {
            get { return left; }

            set
            {
                left = value;
                UpdatePosition();
            }
        }

        public double Top
        {
            get { return top; }

            set
            {
                top = value;
                UpdatePosition();
            }
        }

        public double VerticalOffset
        {
            get { return verticalOffset; }

            set
            {
                verticalOffset = value;
                UpdatePosition();
            }
        }

        [NotNull]
        protected OverlayCanvas Canvas { get; }

        public event EventHandler<EventArgs> Closed;

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                IsOpen = false;
                e.Handled = true;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([NotNull] string propertyName)
        {
            Debug.ArgumentNotNull(propertyName, nameof(propertyName));

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void HandleMoveDelta([NotNull] object sender, [NotNull] DragDeltaEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            VerticalOffset = VerticalOffset + e.VerticalChange;
            HorizontalOffset = HorizontalOffset + e.HorizontalChange;
        }

        private void HandleResizeCompleted([NotNull] object sender, [NotNull] DragCompletedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.SetDouble("AppBuilder", "DesignerWidth", Width);
            AppHost.Settings.SetDouble("AppBuilder", "DesignerHeight", Height);
        }

        private void HandleResizeDelta([NotNull] object sender, [NotNull] DragDeltaEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Width = Width + e.HorizontalChange;
            Height = Height + e.VerticalChange;
        }

        private void RaiseClosed()
        {
            var handler = Closed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void UpdatePosition()
        {
            var x = left + horizontalOffset;
            var y = top + verticalOffset;

            Margin = new Thickness(x, y, 0, 0);
        }
    }
}
