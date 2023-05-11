using System;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;

namespace Avalonia.Controls.Primitives
{
    [PseudoClasses(":selected")]
    public class TreeDataGridRow : TemplatedControl, ISelectable
    {
        private const double DragDistance = 3;
        private static readonly Point s_InvalidPoint = new(double.NegativeInfinity, double.NegativeInfinity);

        public static readonly DirectProperty<TreeDataGridRow, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRow, IColumns?>(
                nameof(Columns),
                o => o.Columns);

        public static readonly DirectProperty<TreeDataGridRow, TreeDataGridElementFactory?> ElementFactoryProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRow, TreeDataGridElementFactory?>(
                nameof(ElementFactory),
                o => o.ElementFactory,
                (o, v) => o.ElementFactory = v);

        public static readonly DirectProperty<TreeDataGridRow, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRow, bool>(
                nameof(IsSelected),
                o => o.IsSelected,
                (o, v) => o.IsSelected = v);

        public static readonly DirectProperty<TreeDataGridRow, IRows?> RowsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRow, IRows?>(
                nameof(Rows),
                o => o.Rows);

        private IColumns? _columns;
        private TreeDataGridElementFactory? _elementFactory;
        private bool _isSelected;
        private IRows? _rows;
        private Point _mouseDownPosition = s_InvalidPoint;

        public IColumns? Columns
        {
            get => _columns;
            private set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        public TreeDataGridElementFactory? ElementFactory
        {
            get => _elementFactory;
            set => SetAndRaise(ElementFactoryProperty, ref _elementFactory, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
        }

        public object? Model => DataContext;

        public IRows? Rows
        {
            get => _rows;
            private set => SetAndRaise(RowsProperty, ref _rows, value);
        }

        public TreeDataGridCellsPresenter? CellsPresenter { get; private set; }
        public int RowIndex { get; private set; }

        public void Realize(
            TreeDataGridElementFactory? elementFactory,
            IColumns? columns,
            IRows? rows,
            int rowIndex)
        {
            ElementFactory = elementFactory;
            Columns = columns;
            Rows = rows;
            DataContext = rows?[rowIndex].Model;
            UpdateIndex(rowIndex);
        }

        public Control? TryGetCell(int columnIndex)
        {
            return CellsPresenter?.TryGetElement(columnIndex);
        }

        public void UpdateIndex(int index)
        {
            RowIndex = index;
            CellsPresenter?.UpdateRowIndex(index);
        }

        public void Unrealize()
        {
            RowIndex = -1;
            DataContext = null;
            CellsPresenter?.Unrealize();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            CellsPresenter = e.NameScope.Find<TreeDataGridCellsPresenter>("PART_CellsPresenter");

            if (RowIndex >= 0)
                CellsPresenter?.Realize(RowIndex);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            _mouseDownPosition = !e.Handled ? e.GetPosition(this) : s_InvalidPoint;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            var delta = e.GetPosition(this) - _mouseDownPosition;

            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || 
                e.Handled ||
                Math.Abs(delta.X) < DragDistance && Math.Abs(delta.Y) < DragDistance ||
                _mouseDownPosition == s_InvalidPoint)
                return;

            _mouseDownPosition = s_InvalidPoint;

            var presenter = Parent as TreeDataGridRowsPresenter;
            var owner = presenter?.TemplatedParent as TreeDataGrid;
            owner?.RaiseRowDragStarted(e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsSelectedProperty)
            {
                PseudoClasses.Set(":selected", IsSelected);
            }
            
            base.OnPropertyChanged(change);
        }
    }
}
