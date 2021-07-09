using System;
using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// An implementation of <see cref="IColumns"/> that stores its columns in a list.
    /// </summary>
    public class ColumnList<TModel> : NotifyingListBase<IColumn<TModel>>, IColumns
    {
        private bool _sizeStarColumnsAtEndOfMeasure = true;
        private double _viewportWidth;

        public event EventHandler? LayoutInvalidated;

        public Size CellMeasured(int columnIndex, int rowIndex, Size size)
        {
            var column = this[columnIndex];

            if (column.Width.IsAuto)
            {
                if (!column.ActualWidth.HasValue || size.Width > column.ActualWidth)
                {
                    _sizeStarColumnsAtEndOfMeasure = true;
                    ((ISetColumnLayout)column).SetActualWidth(size.Width);
                    LayoutInvalidated?.Invoke(this, EventArgs.Empty);
                }

                return new Size(column.ActualWidth!.Value, size.Height);
            }
            else if (column.Width.IsAbsolute)
            {
                return new Size(column.Width.Value, size.Height);
            }

            return size;
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
                if (!column.ActualWidth.HasValue)
                    return (-1, -1);
                start += column.ActualWidth.Value;
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
                else if (column.ActualWidth.HasValue)
                {
                    totalMeasured += column.ActualWidth.Value;
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

        public void MeasureFinished()
        {
            if (_sizeStarColumnsAtEndOfMeasure)
                SizeStarColumns();
            _sizeStarColumnsAtEndOfMeasure = false;
        }

        public void SetColumnWidth(int columnIndex, GridLength width)
        {
            var column = this[columnIndex];

            if (width != column.Width)
            {
                ((ISetColumnLayout)column).SetWidth(width);
                LayoutInvalidated?.Invoke(this, EventArgs.Empty);
                SizeStarColumns();
            }
        }

        public void ViewportChanged(Rect viewport)
        {
            if (_viewportWidth != viewport.Width)
            {
                _viewportWidth = viewport.Width;
                SizeStarColumns();
            }
        }

        IColumn IReadOnlyList<IColumn>.this[int index] => this[index];
        IEnumerator<IColumn> IEnumerable<IColumn>.GetEnumerator() => GetEnumerator();

        private void SizeStarColumns()
        {
            var totalStars = 0.0;
            var availableSpace = _viewportWidth;

            foreach (var column in this)
            {
                if (column.Width.IsStar)
                    totalStars += column.Width.Value;
                else if (column.ActualWidth.HasValue)
                    availableSpace -= column.ActualWidth.Value;
            }

            if (totalStars == 0)
                return;

            var invalidated = false;

            foreach (var column in this)
            {
                if (column.Width.IsStar)
                {
                    var actualWidth = Math.Max(0, availableSpace * (column.Width.Value / totalStars));

                    if (column.ActualWidth != actualWidth)
                    {
                        ((ISetColumnLayout)column).SetActualWidth(actualWidth);
                        invalidated = true;
                    }
                }
            }

            if (invalidated)
                LayoutInvalidated?.Invoke(this, EventArgs.Empty);
        }
    }
}
