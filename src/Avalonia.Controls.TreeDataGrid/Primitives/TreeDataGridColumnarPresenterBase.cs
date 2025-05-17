using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Layout;

namespace Avalonia.Controls.Primitives
{
    /// <summary>
    /// Base class for presenters which display data in virtualized columns.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <remarks>
    /// Implements common layout functionality between <see cref="TreeDataGridCellsPresenter"/>
    /// and <see cref="TreeDataGridColumnHeadersPresenter"/>.
    /// </remarks>
    public abstract class TreeDataGridColumnarPresenterBase<TItem> : TreeDataGridPresenterBase<TItem> 
    {
        private double _lastEstimatedElementSizeU = 25;
        
        protected IColumns? Columns => Items as IColumns;

        protected sealed override Size GetInitialConstraint(Control element, int index, Size availableSize)
        {
            var column = (IUpdateColumnLayout)Columns![index];
            return new Size(Math.Min(availableSize.Width, column.MaxActualWidth), availableSize.Height);
        }

        protected override (int index, double position) GetOrEstimateAnchorElementForViewport(
            double viewportStart,
            double viewportEnd,
            int itemCount)
        {
            if (Columns?.GetColumnAt(viewportStart) is (var index and >= 0, var position))
                return (index, position);
            
            if (Columns?.GetOrEstimateColumnAt(viewportStart, viewportEnd, itemCount, StartU,  FirstIndex, ref _lastEstimatedElementSizeU) is { index: >= 0 } res)
                return res;
            
            return base.GetOrEstimateAnchorElementForViewport(viewportStart, viewportEnd, itemCount);
        }

        protected override double EstimateElementSizeU()
        {
            if (Columns is null)
                return _lastEstimatedElementSizeU;

            var result = Columns.EstimateElementSizeU();
            if (result >= 0)
                _lastEstimatedElementSizeU = result;
            
            return _lastEstimatedElementSizeU;
        }

        protected sealed override bool NeedsFinalMeasurePass(int firstIndex, IReadOnlyList<Control?> elements)
        {
            var columns = Columns!;

            columns.CommitActualWidths();

            // We need to do a second measure pass if any of the controls were measured with a width
            // that is greater than the final column width.
            for (var i = 0; i < elements.Count; i++)
            {
                var e = elements[i];
                if (e is not null)
                {
                    var previous = LayoutInformation.GetPreviousMeasureConstraint(e)!.Value;
                    if (previous.Width > columns[i + firstIndex].ActualWidth)
                        return true;
                }
            }

            return false;
        }

        protected sealed override (int index, double position) GetElementAt(double position)
        {
            return ((IColumns)Items!).GetColumnAt(position);
        }

        protected sealed override Size GetFinalConstraint(Control element, int index, Size availableSize)
        {
            var column = Columns![index];
            return new(column.ActualWidth, double.PositiveInfinity);
        }

        protected sealed override double CalculateSizeU(Size availableSize)
        {
            return Columns?.GetEstimatedWidth(availableSize.Width) ?? 0;
        }
    }
}
