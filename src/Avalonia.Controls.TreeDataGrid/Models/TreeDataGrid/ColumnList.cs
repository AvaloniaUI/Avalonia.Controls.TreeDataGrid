using System;
using System.Collections.Generic;
using Avalonia.Utilities;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// An implementation of <see cref="IColumns"/> that stores its columns in a list.
    /// </summary>
    public class ColumnList<TModel> : NotifyingListBase<IColumn<TModel>>, IColumns
    {
        private bool _sizeStarColumnsAtEndOfMeasure;
        private double _viewportWidth;

        public event EventHandler? LayoutInvalidated;

        public void CellMeasured(int columnIndex, int rowIndex, Size size)
        {
            var column = this[columnIndex];

            if (column.Width.IsAuto && size.Width > column.ActualWidth)
            {
                _sizeStarColumnsAtEndOfMeasure = true;
                ((ISetColumnLayout)column).SetActualWidth(size.Width);
                LayoutInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }

        public (int index, double x) GetColumnAt(double x)
        {
            // TODO: Implement properly.
            if (MathUtilities.IsZero(x))
                return (0, 0);
            return (-1, -1);
        }

        public void MeasureFinished()
        {
            if (_sizeStarColumnsAtEndOfMeasure)
                SizeStarColumns();
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
                else
                    availableSpace -= column.ActualWidth;
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
