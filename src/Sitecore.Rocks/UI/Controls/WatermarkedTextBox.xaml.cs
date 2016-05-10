// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Controls
{
    public partial class WatermarkedTextBox
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(@"Text", typeof(string), typeof(WatermarkedTextBox));

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(@"Watermark", typeof(string), typeof(WatermarkedTextBox));

        private Timer _deferredTextChangedTimer;

        public WatermarkedTextBox()
        {
            InitializeComponent();

            var icon = new Icon("Resources/16x16/close.png");
            CloseIcon.Source = icon.GetSource();

            Unloaded += delegate { StopTimer(); };
        }

        [NotNull]
        public string Text
        {
            get { return GetValue(TextProperty) as string ?? string.Empty; }

            set
            {
                // may be set to null
                SetValue(TextProperty, value);
            }
        }

        [NotNull]
        public string Watermark
        {
            get { return GetValue(WatermarkProperty) as string ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                SetValue(WatermarkProperty, value);
            }
        }

        public event EventHandler DeferredTextChanged;

        public event KeyEventHandler TextBoxKeyDown;

        public event TextChangedEventHandler TextChanged;

        private void Clear([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Text = string.Empty;
        }

        private void HandleTextBoxKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var keyDown = TextBoxKeyDown;
            if (keyDown != null)
            {
                keyDown(sender, e);
            }

            if (e.Key != Key.Escape)
            {
                return;
            }

            TextBox.SelectAll();
            e.Handled = true;
        }

        private void RaiseDeferredTextChanged()
        {
            var deferredTextChanged = DeferredTextChanged;
            if (deferredTextChanged != null)
            {
                deferredTextChanged(this, EventArgs.Empty);
            }
        }

        private void StartTimer()
        {
            StopTimer();
            _deferredTextChangedTimer = new Timer(delegate { Dispatcher.Invoke(RaiseDeferredTextChanged); }, null, 350, int.MaxValue);
        }

        private void StopTimer()
        {
            if (_deferredTextChangedTimer == null)
            {
                return;
            }

            _deferredTextChangedTimer.Dispose();
            _deferredTextChangedTimer = null;
        }

        private void TextBoxChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var changed = TextChanged;
            if (changed != null)
            {
                changed(sender, e);
            }

            StartTimer();
        }
    }
}
