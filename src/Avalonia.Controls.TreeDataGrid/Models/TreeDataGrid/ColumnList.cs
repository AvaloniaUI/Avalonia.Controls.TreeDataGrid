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
        private bool _initialized;
        private double _viewportWidth;

        public event EventHandler? LayoutInvalidated;

        public void AddRange(IEnumerable<IColumn<TModel>> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public Size CellMeasured(int columnIndex, int rowIndex, Size size)
        {
            var column = (IUpdateColumnLayout)this[columnIndex];
            _initialized = true;
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

        public (int index, double position) GetOrEstimateColumnAt(double viewportStartU, double viewportEndU, int itemCount, double startU,
            ref double estimatedElementSizeU)
        {
            // We have no elements, nothing to do here.
            if (itemCount <= 0)
                return (-1, 0);

            // If we're at 0 then display the first item.
            if (MathUtilities.IsZero(viewportStartU))
                return (0, 0);

            int firstIndex = -1;
            for (var i = 0; i < Count; ++i)
            {
                if (!double.IsNaN(this[i].ActualWidth))
                {
                    firstIndex = i;
                    break;
                }
            }

            var u = startU;

            for (var i = 0; i < Count; ++i)
            {
                var size = this[i].ActualWidth;

                if (double.IsNaN(size))
                    break;

                var endU = u + size;

                if (endU > viewportStartU && u < viewportEndU)
                    return (firstIndex + i, u);

                u = endU;
            }

            // We don't have any realized elements in the requested viewport, or can't rely on
            // StartU being valid. Estimate the index using only the estimated size. First,
            // estimate the element size, using defaultElementSizeU if we don't have any realized
            // elements.
            var estimatedSize = EstimateElementSizeU() switch
            {
                -1 => estimatedElementSizeU,
                var v => v,
            };

            // Store the estimated size for the next layout pass.
            estimatedElementSizeU = estimatedSize;

            // Estimate the element at the start of the viewport.
            var index = Math.Min((int)(viewportStartU / estimatedSize), itemCount - 1);
            return (index, index * estimatedSize);
        }
        
        public double EstimateElementSizeU()
        {
            var total = 0.0;
            var divisor = 0.0;

            // Average the size of the realized elements.
            foreach (var column in this)
            {
                var size = column.ActualWidth;
                if (double.IsNaN(size))
                    continue;
                total += size;
                ++divisor;
            }

            // We don't have any elements on which to base our estimate.
            if (divisor == 0 || total == 0)
                return -1;

            return total / divisor;
        }

        public double GetEstimatedWidth(double constraint)
        {
            var hasStar = false;
            var totalMeasured = 0.0;
            var measuredCount = 0;
            var unmeasuredCount = 0;

            for (var i = 0; i < Count; ++i)
            {
                var column = (IUpdateColumnLayout)this[i];

                if (column.Width.IsStar)
                {
                    hasStar = true;
                    totalMeasured += column.MinActualWidth;
                }
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

        public void CommitActualWidths() => UpdateColumnSizes();

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
                if (_initialized)
                    UpdateColumnSizes();
            }
        }

        IColumn IReadOnlyList<IColumn>.this[int index] => this[index];
        IEnumerator<IColumn> IEnumerable<IColumn>.GetEnumerator() => GetEnumerator();

        private void UpdateColumnSizes()
        {
            var totalStars = 0.0;
            var availableSpace = _viewportWidth;
            var invalidated = false;

            // First commit the actual width for all non-star width columns and get a total of the
            // number of stars for star width columns.
            for (var i = 0; i < Count; ++i)
            {
                var column = (IUpdateColumnLayout)this[i];

                if (!column.Width.IsStar)
                {
                    invalidated |= column.CommitActualWidth();
                    availableSpace -= NotNaN(column.ActualWidth);
                }
                else
                    totalStars += column.Width.Value;
            }

            if (totalStars > 0)
            {
                // Size the star columns.
                var starWidthWasConstrained = false;
                var used = 0.0;

                availableSpace = Math.Max(0, availableSpace);

                // Do a first pass to calculate star column widths.
                for (var i = 0; i < Count; ++i)
                {
                    var column = (IUpdateColumnLayout)this[i];

                    if (column.Width.IsStar)
                    {
                        column.CalculateStarWidth(availableSpace, totalStars);
                        used += NotNaN(column.ActualWidth);
                        starWidthWasConstrained |= column.StarWidthWasConstrained;
                    }
                }

                // If the width of any star columns was constrained by their min/max size, and we
                // actually had any space to distribute between star columns, then we need to update
                // the star width for the non-constrained columns.
                if (starWidthWasConstrained && MathUtilities.GreaterThan(availableSpace, 0))
                {
                    for (var i = 0; i < Count; ++i)
                    {
                        var column = (IUpdateColumnLayout)this[i];

                        if (column.StarWidthWasConstrained)
                        {
                            availableSpace -= column.ActualWidth;
                            totalStars -= column.Width.Value;
                        }
                    }

                    for (var i = 0; i < Count; ++i)
                    {
                        var column = (IUpdateColumnLayout)this[i];
                        if (column.Width.IsStar && !column.StarWidthWasConstrained)
                            column.CalculateStarWidth(availableSpace, totalStars);
                    }
                }

                // Finally commit the star column widths.
                for (var i = 0; i < Count; ++i)
                {
                    var column = (IUpdateColumnLayout)this[i];

                    if (column.Width.IsStar)
                    {
                        invalidated |= column.CommitActualWidth();
                    }
                }
            }

            if (invalidated)
                LayoutInvalidated?.Invoke(this, EventArgs.Empty);
        }

        private static double NotNaN(double v) => double.IsNaN(v) ? 0 : v;
    }
}
