using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridCellsPresenter : TreeDataGridPresenterBase<IColumn>
    {
        public static readonly StyledProperty<IBrush> BackgroundProperty =
            TemplatedControl.BackgroundProperty.AddOwner<TreeDataGridCellsPresenter>();

        public static readonly DirectProperty<TreeDataGridCellsPresenter, IRows?> RowsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCellsPresenter, IRows?>(
                nameof(Rows),
                o => o.Rows,
                (o, v) => o.Rows = v);

        private int _rowIndex = -1;
        private IRows? _rows;

        public IBrush Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public int RowIndex
        {
            get => _rowIndex;
            set
            {
                if (_rowIndex != value)
                {
                    _rowIndex = value;
                    RecycleAllElements();
                    InvalidateMeasure();
                }
            }
        }

        public IRows? Rows
        {
            get => _rows;
            set => SetAndRaise(RowsProperty, ref _rows, value);
        }

        protected override Orientation Orientation => Orientation.Horizontal;

        public override void Render(DrawingContext context)
        {
            var background = Background;

            if (background is object)
            {
                context.FillRectangle(background, new Rect(Bounds.Size));
            }
        }

        protected override Size MeasureElement(int index, IControl element, Size availableSize)
        {
            element.Measure(availableSize);
            ((IColumns)Items!).CellMeasured(index, RowIndex, element.DesiredSize);
            return element.DesiredSize;
        }

        protected override Rect ArrangeElement(int index, IControl element, Rect rect)
        {
            var column = ((IColumns)Items!)[index];
            rect = rect.WithWidth(column.ActualWidth);
            element.Arrange(rect);
            return rect;
        }

        protected override IControl GetElementFromFactory(IColumn column, int index)
        {
            var model = _rows!.RealizeCell(column, index, RowIndex);
            var cell = (TreeDataGridCell)GetElementFromFactory(model, index, this);
            cell.Realize(ElementFactory!, model, index, RowIndex);
            return cell;
        }

        protected override (int index, double position) GetElementAt(double position)
        {
            return ((IColumns)Items!).GetColumnAt(position);
        }

        protected override void RealizeElement(IControl element, IColumn column, int index)
        {
            var cell = (TreeDataGridCell)element;

            if (cell.ColumnIndex == index && cell.RowIndex == RowIndex)
            {
                return;
            }
            else if (cell.ColumnIndex == -1 && cell.RowIndex == -1)
            {
                var model = _rows!.RealizeCell(column, index, RowIndex);
                ((TreeDataGridCell)element).Realize(ElementFactory!, model, index, RowIndex);
            }
            else
            {
                throw new InvalidOperationException("Cell already realized");
            }
        }

        protected override void UnrealizeElement(IControl element)
        {
            var cell = (TreeDataGridCell)element;
            var columnIndex = cell.ColumnIndex;
            var rowIndex = cell.RowIndex;
            
            cell.Unrealize();
            _rows!.UnrealizeCell(cell.Model!, columnIndex, rowIndex);
        }

        protected override void UpdateElementIndex(IControl element, int index)
        {
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BackgroundProperty)
                InvalidateVisual();
        }
    }
}
