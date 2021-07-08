using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Layout;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridColumnHeadersPresenter : TreeDataGridPresenterBase<IColumn>
    {
        protected override Orientation Orientation => Orientation.Horizontal;

        protected override Size MeasureElement(int index, IControl element, Size availableSize)
        {
            var columns = (IColumns)Items!;
            element.Measure(availableSize);
            return columns.CellMeasured(index, -1, element.DesiredSize);
        }

        protected override Rect ArrangeElement(int index, IControl element, Rect rect)
        {
            var column = ((IColumns)Items!)[index];
            rect = rect.WithWidth(column.ActualWidth);
            element.Arrange(rect);
            return rect;
        }

        protected override (int index, double position) GetElementAt(double position)
        {
            return ((IColumns)Items!).GetColumnAt(position);
        }

        protected override void RealizeElement(IControl element, IColumn column, int index)
        {
            ((TreeDataGridColumnHeader)element).Realize((IColumns)Items!, index);
        }

        protected override void UpdateElementIndex(IControl element, int index)
        {
        }

        protected override void UnrealizeElement(IControl element)
        {
            ((TreeDataGridColumnHeader)element).Unrealize();
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

        private void OnColumnLayoutInvalidated(object sender, EventArgs e)
        {
            InvalidateMeasure();
        }
    }
}
