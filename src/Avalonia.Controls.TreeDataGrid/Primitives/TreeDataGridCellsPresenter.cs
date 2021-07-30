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

        private IRows? _rows;

        public IBrush Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public IRows? Rows
        {
            get => _rows;
            set => SetAndRaise(RowsProperty, ref _rows, value);
        }

        public int RowIndex { get; private set; } = -1;

        protected override Orientation Orientation => Orientation.Horizontal;

        public void Realize(int index)
        {
            if (RowIndex != -1)
                throw new InvalidOperationException("Row is already realized.");
            UpdateIndex(index);
            InvalidateMeasure();
        }

        public override void Render(DrawingContext context)
        {
            var background = Background;

            if (background is object)
            {
                context.FillRectangle(background, new Rect(Bounds.Size));
            }
        }

        public void Unrealize()
        {
            if (RowIndex == -1)
                throw new InvalidOperationException("Row is not realized.");
            RowIndex = -1;
            RecycleAllElements();
        }

        public void UpdateIndex(int index)
        {
            if (index < 0 || Rows is null || index >= Rows.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            RowIndex = index;
        }

        protected override Size MeasureElement(int index, IControl element, Size availableSize)
        {
            element.Measure(availableSize);
            return ((IColumns)Items!).CellMeasured(index, RowIndex, element.DesiredSize);
        }

        protected override Rect ArrangeElement(int index, IControl element, Rect rect)
        {
            var column = ((IColumns)Items!)[index];

            if (!column.ActualWidth.HasValue)
                throw new AvaloniaInternalException("Attempt to arrange cell before measure.");

            rect = rect.WithWidth(column.ActualWidth.Value);
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

        protected override double CalculateSizeU(Size availableSize)
        {
            if (Items is null)
                return 0;

            return ((IColumns)Items).GetEstimatedWidth(availableSize.Width);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BackgroundProperty)
                InvalidateVisual();
        }
    }
}
