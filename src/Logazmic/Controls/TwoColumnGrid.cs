using System;
using System.Collections.Generic;
using System.Linq;

namespace Logazmic.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public class TwoColumnGrid : Panel
    {
        private double _column1Width;
        private readonly List<double> _rowHeights = new List<double>();

        public TwoColumnGrid()
        {
            Focusable = false;
        }

        /// <summary>
        /// Gets or sets the amount of spacing (in device independent pixels) between the rows.
        /// </summary>
        public double RowSpacing
        {
            get => (double)GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        /// <summary>
        /// Identifies the ColumnSpacing dependency property
        /// </summary>
        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.Register("RowSpacing", typeof(double), typeof(TwoColumnGrid),
            new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the amount of spacing (in device independent pixels) between the columns.
        /// </summary>
        public double ColumnSpacing
        {
            get => (double)GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        /// <summary>
        /// Identifies the ColumnSpacing dependency property
        /// </summary>
        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.Register("ColumnSpacing", typeof(double), typeof(TwoColumnGrid),
            new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));


        /// <summary>
        /// Measures the size required for all the child elements in this panel.
        /// </summary>
        /// <param name="constraint">The size constraint given by our parent.</param>
        /// <returns>The requested size for this panel including all children</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            double col1Width = 0;
            double col2Width = 0;
            _rowHeights.Clear();
            // First, measure all the left column children
            for (int i = 0; i < VisualChildrenCount; i += 2)
            {
                var child = Children[i];
                child.Measure(constraint);
                col1Width = Math.Max(child.DesiredSize.Width, col1Width);
                _rowHeights.Add(child.DesiredSize.Height);
            }
            // Then, measure all the right column children, they get whatever remains in width
            var newWidth = Math.Max(0, constraint.Width - col1Width - ColumnSpacing);
            Size newConstraint = new Size(newWidth, constraint.Height);
            for (int i = 1; i < VisualChildrenCount; i += 2)
            {
                var child = Children[i];
                child.Measure(newConstraint);
                col2Width = Math.Max(child.DesiredSize.Width, col2Width);
                _rowHeights[i / 2] = Math.Max(_rowHeights[i / 2], child.DesiredSize.Height);
            }

            _column1Width = col1Width;
            return new Size(
                col1Width + ColumnSpacing + col2Width,
                _rowHeights.Sum() + ((_rowHeights.Count - 1) * RowSpacing));
        }

        /// <summary>
        /// Position elements and determine the final size for this panel.
        /// </summary>
        /// <param name="arrangeSize">The final area where child elements should be positioned.</param>
        /// <returns>The final size required by this panel</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double y = 0;
            for (int i = 0; i < VisualChildrenCount; i++)
            {
                var child = Children[i];
                double height = _rowHeights[i / 2];
                if (i % 2 == 0)
                {
                    // Left child
                    var r = new Rect(0, y, _column1Width, height);
                    child.Arrange(r);
                }
                else
                {
                    // Right child
                    var r = new Rect(_column1Width + ColumnSpacing, y, arrangeSize.Width - _column1Width - ColumnSpacing, height);
                    child.Arrange(r);
                    y += height;
                    y += RowSpacing;
                }
            }
            return base.ArrangeOverride(arrangeSize);
        }

    }
}
