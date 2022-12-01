using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridColumnHeadersPresenter : TreeDataGridColumnarPresenterBase<IColumn>, IChildIndexProvider
    {
        public event EventHandler<ChildIndexChangedEventArgs>? ChildIndexChanged;

        protected override Orientation Orientation => Orientation.Horizontal;

        protected override Size ArrangeOverride(Size finalSize)
        {
            (Items as IColumns)?.CommitActualWidths();
            return base.ArrangeOverride(finalSize);
        }

        protected override Size MeasureElement(int index, IControl element, Size availableSize)
        {
            var columns = (IColumns)Items!;
            element.Measure(availableSize);
            return columns.CellMeasured(index, -1, element.DesiredSize);
        }

        protected override void RealizeElement(IControl element, IColumn column, int index)
        {
            ((TreeDataGridColumnHeader)element).Realize((IColumns)Items!, index);
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element));
        }

        protected override void UpdateElementIndex(IControl element, int index)
        {
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element));
        }

        protected override void UnrealizeElement(IControl element)
        {
            ((TreeDataGridColumnHeader)element).Unrealize();
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element));
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property == ItemsProperty)
            {
                var oldValue = change.OldValue.GetValueOrDefault<IColumns>();
                var newValue = change.NewValue.GetValueOrDefault<IColumns>();

                if (oldValue is object)
                    oldValue.LayoutInvalidated -= OnColumnLayoutInvalidated;
                if (newValue is object)
                    newValue.LayoutInvalidated += OnColumnLayoutInvalidated;
            }

            base.OnPropertyChanged(change);
        }

        private void OnColumnLayoutInvalidated(object? sender, EventArgs e)
        {
            InvalidateMeasure();
        }

        public int GetChildIndex(ILogical child)
        {
            if (child is TreeDataGridColumnHeader header)
            {
                return header.ColumnIndex;
            }
            return -1;
        }

        public bool TryGetTotalCount(out int count)
        {
            if (Items is null)
            {
                count = 0;
                return false;
            }

            count = Items.Count;
            return true;
        }
    }
}
