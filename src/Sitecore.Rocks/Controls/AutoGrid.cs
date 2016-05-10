// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public class AutoGrid : Grid
    {
        public static readonly DependencyProperty ChildHorizontalAlignmentProperty = DependencyProperty.Register("ChildHorizontalAlignment", typeof(HorizontalAlignment?), typeof(AutoGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));

        public static readonly DependencyProperty ChildMarginProperty = DependencyProperty.Register("ChildMargin", typeof(Thickness?), typeof(AutoGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));

        public static readonly DependencyProperty ChildVerticalAlignmentProperty = DependencyProperty.Register("ChildVerticalAlignment", typeof(VerticalAlignment?), typeof(AutoGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));

        public static readonly DependencyProperty IsAutoIndexingProperty = DependencyProperty.Register("IsAutoIndexing", typeof(bool), typeof(AutoGrid), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AutoGrid), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));

        private int _rowOrColumnCount;

        private bool _shouldReindex = true;

        public HorizontalAlignment? ChildHorizontalAlignment
        {
            get { return (HorizontalAlignment?)GetValue(ChildHorizontalAlignmentProperty); }

            set { SetValue(ChildHorizontalAlignmentProperty, value); }
        }

        public Thickness? ChildMargin
        {
            get { return (Thickness?)GetValue(ChildMarginProperty); }

            set { SetValue(ChildMarginProperty, value); }
        }

        public VerticalAlignment? ChildVerticalAlignment
        {
            get { return (VerticalAlignment?)GetValue(ChildVerticalAlignmentProperty); }

            set { SetValue(ChildVerticalAlignmentProperty, value); }
        }

        public bool IsAutoIndexing
        {
            get { return (bool)GetValue(IsAutoIndexingProperty); }

            set { SetValue(IsAutoIndexingProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }

            set { SetValue(OrientationProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var isVertical = Orientation == Orientation.Vertical;

            if (_shouldReindex || (IsAutoIndexing && ((isVertical && _rowOrColumnCount != ColumnDefinitions.Count) || (!isVertical && _rowOrColumnCount != RowDefinitions.Count))))
            {
                _shouldReindex = false;

                if (IsAutoIndexing)
                {
                    _rowOrColumnCount = isVertical ? ColumnDefinitions.Count : RowDefinitions.Count;
                    if (_rowOrColumnCount == 0)
                    {
                        _rowOrColumnCount = 1;
                    }

                    var cellCount = 0;
                    foreach (UIElement child in Children)
                    {
                        cellCount += isVertical ? GetColumnSpan(child) : GetRowSpan(child);
                    }

                    // Update the number of rows/columns
                    if (isVertical)
                    {
                        var newRowCount = (cellCount - 1) / _rowOrColumnCount + 1;
                        while (RowDefinitions.Count < newRowCount)
                        {
                            RowDefinitions.Add(new RowDefinition());
                        }

                        if (RowDefinitions.Count > newRowCount)
                        {
                            RowDefinitions.RemoveRange(newRowCount, RowDefinitions.Count - newRowCount);
                        }
                    }
                    else
                    {
                        // horizontal
                        var newColumnCount = (cellCount - 1) / _rowOrColumnCount + 1;
                        while (ColumnDefinitions.Count < newColumnCount)
                        {
                            ColumnDefinitions.Add(new ColumnDefinition());
                        }

                        if (ColumnDefinitions.Count > newColumnCount)
                        {
                            ColumnDefinitions.RemoveRange(newColumnCount, ColumnDefinitions.Count - newColumnCount);
                        }
                    }
                }

                // Update children indices
                var position = 0;
                foreach (UIElement child in Children)
                {
                    if (IsAutoIndexing)
                    {
                        if (isVertical)
                        {
                            SetRow(child, position / _rowOrColumnCount);
                            SetColumn(child, position % _rowOrColumnCount);
                            position += GetColumnSpan(child);
                        }
                        else
                        {
                            SetRow(child, position % _rowOrColumnCount);
                            SetColumn(child, position / _rowOrColumnCount);
                            position += GetRowSpan(child);
                        }
                    }

                    // Set margin and alignment
                    if (ChildMargin != null)
                    {
                        child.SetIfDefault(MarginProperty, ChildMargin.Value);
                    }

                    if (ChildHorizontalAlignment != null)
                    {
                        child.SetIfDefault(HorizontalAlignmentProperty, ChildHorizontalAlignment.Value);
                    }

                    if (ChildVerticalAlignment != null)
                    {
                        child.SetIfDefault(VerticalAlignmentProperty, ChildVerticalAlignment.Value);
                    }
                }
            }

            return base.MeasureOverride(constraint);
        }

        protected override void OnVisualChildrenChanged([CanBeNull] DependencyObject visualAdded, [CanBeNull] DependencyObject visualRemoved)
        {
            _shouldReindex = true;

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        private static void OnPropertyChanged([NotNull] DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(o, nameof(o));

            ((AutoGrid)o)._shouldReindex = true;
        }
    }
}
