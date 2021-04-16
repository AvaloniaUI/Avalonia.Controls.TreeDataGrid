using System;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    [PseudoClasses(":selected")]
    public class TreeDataGridRow : TemplatedControl, ISelectable
    {
        public static readonly DirectProperty<TreeDataGridRow, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRow, IColumns?>(
                nameof(Columns),
                o => o.Columns);

        public static readonly DirectProperty<TreeDataGridRow, IElementFactory?> ElementFactoryProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRow, IElementFactory?>(
                nameof(ElementFactory),
                o => o.ElementFactory,
                (o, v) => o.ElementFactory = v);

        public static readonly DirectProperty<TreeDataGridCell, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCell, bool>(
                nameof(IsSelected),
                o => o.IsSelected,
                (o, v) => o.IsSelected = v);

        public static readonly DirectProperty<TreeDataGridRow, IRows?> RowsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRow, IRows?>(
                nameof(Rows),
                o => o.Rows);

        private IColumns? _columns;
        private IElementFactory? _elementFactory;
        private bool _isSelected;
        private IRows? _rows;

        public IColumns? Columns
        {
            get => _columns;
            private set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        public IElementFactory? ElementFactory
        {
            get => _elementFactory;
            set => SetAndRaise(ElementFactoryProperty, ref _elementFactory, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
        }

        public IRows? Rows
        {
            get => _rows;
            private set => SetAndRaise(RowsProperty, ref _rows, value);
        }

        public TreeDataGridCellsPresenter? CellsPresenter { get; private set; }
        public int RowIndex { get; private set; }

        public void Realize(
            IElementFactory? elementFactory,
            IColumns? columns,
            IRows? rows,
            int rowIndex)
        {
            ElementFactory = elementFactory;
            Columns = columns;
            Rows = rows;
            UpdateIndex(rowIndex);
        }

        public void UpdateIndex(int index)
        {
            RowIndex = index;
            CellsPresenter?.UpdateIndex(index);
        }

        public void Unrealize()
        {
            RowIndex = -1;
            CellsPresenter?.Unrealize();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            CellsPresenter = e.NameScope.Find<TreeDataGridCellsPresenter>("PART_CellsPresenter");

            if (RowIndex >= 0)
                CellsPresenter?.Realize(RowIndex);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property == IsSelectedProperty)
            {
                PseudoClasses.Set(":selected", change.NewValue.GetValueOrDefault<bool>());
            }
        }
    }
}
