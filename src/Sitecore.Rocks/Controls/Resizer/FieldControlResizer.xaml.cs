// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.Resizer
{
    public partial class FieldControlResizer
    {
        public static readonly DependencyProperty DefaultHeightProperty = DependencyProperty.Register(@"DefaultHeight", typeof(double), typeof(FieldControlResizer), new PropertyMetadata(0.0));

        public static readonly DependencyProperty FieldIdProperty = DependencyProperty.Register(@"FieldId", typeof(FieldId), typeof(FieldControlResizer));

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(@"Target", typeof(FrameworkElement), typeof(FieldControlResizer));

        public FieldControlResizer()
        {
            DefaultHeight = 100;

            InitializeComponent();

            Loaded += ControlLoaded;
        }

        public double DefaultHeight
        {
            get { return (double)GetValue(DefaultHeightProperty); }

            set { SetValue(DefaultHeightProperty, value); }
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

        protected double TargetHeight { get; set; }

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

            var height = DefaultHeight;

            var fieldId = FieldId;
            if (fieldId != null)
            {
                var value = AppHost.Settings.Get("ContentEditor\\Fields", "Field_" + fieldId.ToShortId(), height.ToString()) as string ?? string.Empty;

                double h;
                if (double.TryParse(value, out h))
                {
                    height = h;
                }
            }

            target.Height = height;
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
            TargetHeight = target.ActualHeight;

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

            var dy = position.Y - Position.Y;
            var height = TargetHeight + dy;

            if (height < 16 || height > 800)
            {
                return;
            }

            target.Height = height;
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
                AppHost.Settings.Set("ContentEditor\\Fields", "Field_" + fieldId.ToShortId(), target.ActualHeight.ToString());
            }
        }
    }
}
