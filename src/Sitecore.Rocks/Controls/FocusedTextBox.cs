// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public class FocusedTextBox : TextBox
    {
        public delegate ValidationStructure ValidationDelegate(ref string value);

        private const double CursorLeftPadding = 2.0;

        private bool _isSelecting;

        private bool _justFocused;

        private bool _manipulatingText;

        private int _selectingSelectionStart;

        private int _selectionLengthBuffer;

        private int _selectionStartBuffer;

        public FocusedTextBox()
        {
            CurrentText = string.Empty;
            GotFocus += MyGotFocus;
            LostFocus += MyLostFocus;
            PreviewMouseLeftButtonUp += MyPreviewMouseLeftButtonUp;
            PreviewMouseLeftButtonDown += MyPreviewMouseLeftButtonDown;
            SelectionChanged += BufferSelection;
            TextChanged += MyTextChanged;
            Cursor = Cursors.Arrow;
            PreviewMouseMove += MyPreviewMouseMove;
        }

        public string CurrentText { get; set; }

        public ValidationDelegate Validator { get; set; }

        public void RestoreBufferedSelection()
        {
            SelectionStart = _selectionStartBuffer;
            SelectionLength = _selectionLengthBuffer;
        }

        public void RestoreBufferedText()
        {
            Text = CurrentText;
            RestoreBufferedSelection();
        }

        private void BufferSelection([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            _selectionStartBuffer = SelectionStart;
            _selectionLengthBuffer = SelectionLength;
        }

        private void ExtendSelectionToPositionInText(double positionInText)
        {
            var x = FindCursorCharacter(positionInText);
            var y = x - _selectingSelectionStart;
            if (y < 0)
            {
                SelectionStart = x;
                y *= -1;
            }
            SelectionLength = y;
        }

        private int FindCursorCharacter(double relativePosition)
        {
            // Find Cursor
            var currentTypeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var newSelectionStart = 0;
            for (var i = 1; i < Text.Length; ++i)
            {
#pragma warning disable CS0618 // Type or member is obsolete
				var currentTextWidth = new FormattedText(Text.Substring(0, i), CultureInfo.CurrentCulture, FlowDirection, currentTypeface, FontSize, Foreground).Width;
#pragma warning restore CS0618 // Type or member is obsolete
				if (relativePosition > currentTextWidth - CursorLeftPadding)
                {
                    newSelectionStart = i;
                }
                else
                {
                    break;
                }
            }
            return newSelectionStart;
        }

        private void MyGotFocus([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            SelectAll();
            Cursor = Cursors.IBeam;
            _justFocused = true;
        }

        private void MyLostFocus([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            Cursor = Cursors.Arrow;
        }

        private void MyPreviewMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            // If double clicked on a focused field, defocus the field and fix the cursor.
            if (e.ClickCount == 2 && SelectionLength == Text.Length)
            {
                PlaceCursorAtRelativePositionInText(e.GetPosition(this).X);
                _isSelecting = true;
                _selectingSelectionStart = SelectionStart;
                e.Handled = true;
                return;
            }

            // Focus field on first click.
            if (!IsFocused)
            {
                e.Handled = Focus();
            }
        }

        private void MyPreviewMouseLeftButtonUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            _isSelecting = false;

            if (e.ClickCount == 2)
            {
                e.Handled = true;
            }

            if (!_justFocused)
            {
                return;
            }

            if (SelectionLength < Text.Length)
            {
                SelectAll();
            }

            _justFocused = false;
            e.Handled = true;
        }

        private void MyPreviewMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            if (_isSelecting)
            {
                ExtendSelectionToPositionInText(e.GetPosition(this).X);
            }
        }

        private void MyTextChanged([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            if (_manipulatingText)
            {
                e.Handled = true;
                return;
            }

            if (Validator == null)
            {
                CurrentText = Text;
                return;
            }

            var validValue = Text;
            var validation = Validator(ref validValue);

            if (!validation.Valid)
            {
                RestoreBufferedText();
                return;
            }

            if (validation.Changed)
            {
                _manipulatingText = true;
                Text = validValue;
                _manipulatingText = false;
            }

            CurrentText = validValue;
        }

        private void PlaceCursorAtRelativePositionInText(double positionInText)
        {
            // Else handle by resetting the selection to start at current cursor position.
            SelectionLength = 0;

            // Place cursor
            SelectionStart = FindCursorCharacter(positionInText);
        }

        public struct ValidationStructure
        {
            public bool Changed;

            public bool Valid;

            public ValidationStructure(bool valid, bool changed)
            {
                Valid = valid;
                Changed = changed;
            }
        }
    }
}
