using System;
using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// An implementation of <see cref="IColumns"/> that stores its columns in a list.
    /// </summary>
    public class ColumnList<TModel> : NotifyingListBase<IColumn<TModel>>, IColumns
    {
        private double _viewportWidth;

        public event EventHandler? LayoutInvalidated;

        public Size CellMeasured(int columnIndex, int rowIndex, Size size)
        {
            var column = (IUpdateColumnLayout)this[columnIndex];
            return new Size(column.CellMeasured(size.Width, rowIndex), size.Height);
        }

        public (int index, double x) GetColumnAt(double x)
        {
            var start = 0.0;

            for (var i = 0; i < Count; ++i)
            {
                var column = this[i];
                var end = start + column.ActualWidth;
                if (x >= start && x < end)
                    return (i, start);
                if (double.IsNaN(column.ActualWidth))
                    return (-1, -1);
                start += column.ActualWidth;
            }

            return (-1, -1);
        }

        public double GetEstimatedWidth(double constraint)
        {
            var hasStar = false;
            var totalMeasured = 0.0;
            var measuredCount = 0;
            var unmeasuredCount = 0;

            foreach (var column in this)
            {
                if (column.Width.IsStar)
                    hasStar = true;
                else if (!double.IsNaN(column.ActualWidth))
                {
                    totalMeasured += column.ActualWidth;
                    ++measuredCount;
                }
                else
                    ++unmeasuredCount;
            }

            // If there are star columns, and all measured columns fit within the available space
            // then we will fill the available space.
            if (hasStar && !double.IsInfinity(constraint) && totalMeasured < constraint)
                return constraint;

            // If there are a mix of measured and unmeasured columns then use the measured columns
            // to estimate the size of the unmeasured columns.
            if (measuredCount > 0 && unmeasuredCount > 0)
            {
                var estimated = (totalMeasured / measuredCount) * unmeasuredCount;
                return totalMeasured + estimated;
            }

            return totalMeasured;
        }

        public void MeasureEnd() => UpdateColumnSizes();

        public void SetColumnWidth(int columnIndex, GridLength width)
        {
            var column = this[columnIndex];

            if (width != column.Width)
            {
                ((IUpdateColumnLayout)column).SetWidth(width);
                LayoutInvalidated?.Invoke(this, EventArgs.Empty);
                UpdateColumnSizes();
            }
        }

        public void ViewportChanged(Rect viewport)
        {
            if (_viewportWidth != viewport.Width)
            {
                _viewportWidth = viewport.Width;
                UpdateColumnSizes();
            }
        }

        IColumn IReadOnlyList<IColumn>.this[int index] => this[index];
        IEnumerator<IColumn> IEnumerable<IColumn>.GetEnumerator() => GetEnumerator();

        private void UpdateColumnSizes()
        {
            var totalStars = 0.0;
            var availableSpace = _viewportWidth;

            foreach (IUpdateColumnLayout column in this)
            {
                if (!column.Width.IsStar)
                {
                    column.CommitActualWidth();
                    availableSpace -= column.ActualWidth;
                }
                else
                    totalStars += column.Width.Value;
            }

            if (totalStars == 0)
                return;

            var invalidated = false;

            foreach (IUpdateColumnLayout column in this)
            {
                if (column.Width.IsStar)
                {
                    var oldSize = column.ActualWidth;
                    column.CommitActualWidth(availableSpace, totalStars);
                    invalidated |= oldSize != column.ActualWidth;
                }
            }

            if (invalidated)
                LayoutInvalidated?.Invoke(this, EventArgs.Empty);
        }
    }
}
