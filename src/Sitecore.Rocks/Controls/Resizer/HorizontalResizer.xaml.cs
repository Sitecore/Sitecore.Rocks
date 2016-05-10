// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.Resizer
{
    public partial class HorizontalResizer
    {
        public static readonly DependencyProperty DefaultWidthProperty = DependencyProperty.Register(@"DefaultWidth", typeof(double), typeof(HorizontalResizer), new PropertyMetadata(0.0));

        public static readonly DependencyProperty FieldIdProperty = DependencyProperty.Register(@"FieldId", typeof(FieldId), typeof(HorizontalResizer));

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(@"Target", typeof(FrameworkElement), typeof(HorizontalResizer));

        public HorizontalResizer()
        {
            DefaultWidth = 100;

            InitializeComponent();

            Loaded += ControlLoaded;
        }

        public double DefaultWidth
        {
            get { return (double)GetValue(DefaultWidthProperty); }

            set { SetValue(DefaultWidthProperty, value); }
        }

        [CanBeNull]
        public FieldId FieldId
        {
            get { return GetValue(FieldIdProperty) as FieldId; }

            set { SetValue(FieldIdProperty, value); }
        }

        [CanBeNull]
        public FrameworkElement Target
        {
            get { return GetValue(TargetProperty) as FrameworkElement; }

            set { SetValue(TargetProperty, value); }
        }

        protected bool IsMouseDown { get; set; }

        protected Point Position { get; set; }

        protected double TargetWidth { get; set; }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var target = Target;
            if (target == null)
            {
                return;
            }

            var width = DefaultWidth;

            var fieldId = FieldId;
            if (fieldId != null)
            {
                var value = AppHost.Settings.Get("ContentEditor\\Fields", "Field_Horizontal_" + fieldId.ToShortId(), width.ToString()) as string ?? string.Empty;

                double h;
                if (double.TryParse(value, out h))
                {
                    width = h;
                }
            }

            target.Width = width;
        }

        private void HandleMouseDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var target = Target;
            if (target == null)
            {
                return;
            }

            IsMouseDown = true;
            Position = PointToScreen(e.GetPosition(this));
            TargetWidth = target.ActualWidth;

            Mouse.Capture(ResizeBar, CaptureMode.Element);
            e.Handled = true;
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!IsMouseDown)
            {
                return;
            }

            var target = Target;
            if (target == null)
            {
                return;
            }

            var position = PointToScreen(e.GetPosition(this));

            var dy = position.X - Position.X;
            var width = TargetWidth + dy;

            if (width < 16 || width > 800)
            {
                return;
            }

            target.Width = width;
            e.Handled = true;
        }

        private void HandleMouseUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsMouseDown = false;
            Mouse.Capture(null);
            e.Handled = true;

            var fieldId = FieldId;
            var target = Target;

            if (fieldId != null && target != null)
            {
                AppHost.Settings.Set("ContentEditor\\Fields", "Field_Horizontal_" + fieldId.ToShortId(), target.ActualWidth.ToString());
            }
        }
    }
}
