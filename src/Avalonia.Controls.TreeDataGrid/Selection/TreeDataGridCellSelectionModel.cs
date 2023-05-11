using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Avalonia.Controls.Selection
{
    public class TreeDataGridCellSelectionModel<TModel> : ITreeDataGridCellSelectionModel<TModel>,
        ITreeDataGridSelectionInteraction
        where TModel : class
    {
        private static readonly Point s_InvalidPoint = new(double.NegativeInfinity, double.NegativeInfinity);
        private readonly ITreeDataGridColumnSelectionModel _selectedColumns;
        private readonly ITreeDataGridRowSelectionModel<TModel> _selectedRows;
        private readonly SelectedCellIndexes _selectedIndexes;
        private readonly ITreeDataGridSource<TModel> _source;
        private EventHandler<TreeDataGridCellSelectionChangedEventArgs>? _untypedSelectionChanged;
        private EventHandler? _viewSelectionChanged;
        private Point _pressedPoint = s_InvalidPoint;
        private (int x, int y) _rangeAnchor = (-1, -1);
        private bool _columnsChanged;
        private bool _rowsChanged;

        public TreeDataGridCellSelectionModel(ITreeDataGridSource<TModel> source)
        {
            _source = source;
            _selectedColumns = new TreeDataGridColumnSelectionModel(source.Columns);
            _selectedRows = new TreeDataGridRowSelectionModel<TModel>(source);
            _selectedColumns.SelectionChanged += OnSelectedColumnsSelectionChanged;
            _selectedRows.SelectionChanged += OnSelectedRowsSelectionChanged;
            _selectedIndexes = new(_selectedColumns, _selectedRows);
        }

        public int Count => _selectedColumns.Count * _selectedRows.Count;

        public bool SingleSelect
        {
            get => _selectedRows.SingleSelect;
            set => _selectedColumns.SingleSelect = _selectedRows.SingleSelect = value;
        }

        public CellIndex SelectedIndex
        {
            get => new(_selectedColumns.SelectedIndex, _selectedRows.SelectedIndex);
            set
            {
                var rowIndex = _source.Rows.ModelIndexToRowIndex(value.RowIndex);
                Select(value.ColumnIndex, rowIndex, value.RowIndex);
            }
        }

        public IReadOnlyList<CellIndex> SelectedIndexes => _selectedIndexes;

        IEnumerable? ITreeDataGridSelection.Source
        {
            get => ((ITreeDataGridSelection)_selectedRows).Source;
            set => ((ITreeDataGridSelection)_selectedRows).Source = value;
        }

        public event EventHandler<TreeDataGridCellSelectionChangedEventArgs<TModel>>? SelectionChanged;

        event EventHandler? ITreeDataGridSelectionInteraction.SelectionChanged
        {
            add => _viewSelectionChanged += value;
            remove => _viewSelectionChanged -= value;
        }

        event EventHandler<TreeDataGridCellSelectionChangedEventArgs>? ITreeDataGridCellSelectionModel.SelectionChanged
        {
            add => _untypedSelectionChanged += value;
            remove => _untypedSelectionChanged -= value;
        }

        public bool IsSelected(int columnIndex, IndexPath rowIndex)
        {
            return _selectedColumns.IsSelected(columnIndex) && _selectedRows.IsSelected(rowIndex);
        }

        bool ITreeDataGridSelectionInteraction.IsCellSelected(int columnIndex, int rowIndex)
        {
            return IsSelected(columnIndex, rowIndex);
        }

        void ITreeDataGridSelectionInteraction.OnKeyDown(TreeDataGrid sender, KeyEventArgs e)
        {
            var direction = e.Key.ToNavigationDirection();
            var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

            if (sender.RowsPresenter is null ||
                sender.Columns is null ||
                sender.Rows is null ||
                e.Handled || !direction.HasValue)
                return;

            var (x, y) = direction switch
            {
                NavigationDirection.Up => (0, -1),
                NavigationDirection.Down => (0, 1),
                NavigationDirection.Left => (-1, 0),
                NavigationDirection.Right => (1, 0),
                _ => (0, 0)
            };

            var columnIndex = Math.Clamp(_rangeAnchor.x + x, 0, sender.Columns.Count - 1);
            var rowIndex = Math.Clamp(_rangeAnchor.y + y, 0, sender.Rows.Count - 1);

            if (!shift)
                Select(columnIndex, rowIndex);
            else
                SelectFromAnchorTo(columnIndex, rowIndex);
        }

        void ITreeDataGridSelectionInteraction.OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e)
        {
            // Select a cell on pointer pressed if:
            //
            // - It's a mouse click, not touch: we don't want to select on touch scroll gesture start
            // - The cell isn't already selected: we don't want to deselect an existing multiple selection
            //   if the user is trying to drag multiple cells
            //
            // Otherwise select on pointer release.
            if (!e.Handled &&
                e.Pointer.Type == PointerType.Mouse &&
                e.Source is Control source &&
                sender.TryGetCell(source, out var cell) &&
                !IsSelected(cell.ColumnIndex, cell.RowIndex))
            {
                PointerSelect(sender, cell, e);
            }
            else
            {
                _pressedPoint = e.GetPosition(sender);
            }
        }

        void ITreeDataGridSelectionInteraction.OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e)
        {
            if (!e.Handled &&
                _pressedPoint != s_InvalidPoint &&
                e.Source is Control source &&
                sender.TryGetCell(source, out var cell))
            {
                var p = e.GetPosition(sender);
                if (Math.Abs(p.X - _pressedPoint.X) <= 3 || Math.Abs(p.Y - _pressedPoint.Y) <= 3)
                    PointerSelect(sender, cell, e);
            }
        }

        private void BeginBatchUpdate()
        {
            _selectedColumns.BeginBatchUpdate();
            _selectedRows.BeginBatchUpdate();
        }

        private void EndBatchUpdate()
        {
            _columnsChanged = _rowsChanged = false;
            _selectedColumns.EndBatchUpdate();
            _selectedRows.EndBatchUpdate();

            if (_columnsChanged || _rowsChanged)
            {
                var e = new TreeDataGridCellSelectionChangedEventArgs<TModel>();
                _viewSelectionChanged?.Invoke(this, EventArgs.Empty);
                SelectionChanged?.Invoke(this, e);
                _untypedSelectionChanged?.Invoke(this, e);
            }
        }

        private void PointerSelect(TreeDataGrid sender, TreeDataGridCell cell, PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(sender);
            var isRightButton = point.Properties.PointerUpdateKind is PointerUpdateKind.RightButtonPressed or
                PointerUpdateKind.RightButtonReleased;

            UpdateSelection(
                sender,
                cell.ColumnIndex,
                cell.RowIndex,
                rangeModifier: e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                rightButton: isRightButton);
            e.Handled = true;
        }

        private void UpdateSelection(
            TreeDataGrid treeDataGrid,
            int columnIndex,
            int rowIndex,
            bool rangeModifier = false,
            bool rightButton = false)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);

            if (modelIndex == default)
                return;

            var multi = !SingleSelect;
            var range = multi && rangeModifier;

            if (rightButton)
            {
                if (IsSelected(columnIndex, modelIndex) == false && !treeDataGrid.QueryCancelSelection())
                    Select(columnIndex, rowIndex, modelIndex);
            }
            else if (range)
            {
                if (!treeDataGrid.QueryCancelSelection())
                    SelectFromAnchorTo(columnIndex, rowIndex);
            }
            else if (_selectedColumns.SelectedIndex != columnIndex || 
                _selectedRows.SelectedIndex != modelIndex ||
                Count > 1)
            {
                if (!treeDataGrid.QueryCancelSelection())
                    Select(columnIndex, rowIndex, modelIndex);
            }
        }

        private void Select(int columnIndex, int rowIndex)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);
            Select(columnIndex, rowIndex, modelIndex);
        }

        private void Select(int columnIndex, int rowIndex, IndexPath modelndex)
        {
            BeginBatchUpdate();
            _selectedColumns.SelectedIndex = columnIndex;
            _selectedRows.SelectedIndex = modelndex;
            _rangeAnchor = (columnIndex, rowIndex);
            EndBatchUpdate();
        }

        private void SelectFromAnchorTo(int columnIndex, int rowIndex)
        {
            var anchorColumnIndex = _selectedColumns.AnchorIndex;
            var anchorModelIndex = _selectedRows.AnchorIndex;
            var anchorRowIndex = _source.Rows.ModelIndexToRowIndex(anchorModelIndex);

            BeginBatchUpdate();

            _selectedColumns.SelectedIndex = anchorColumnIndex;
            _selectedColumns.SelectRange(anchorColumnIndex, columnIndex);
            _selectedRows.SelectedIndex = anchorModelIndex;

            for (var i = Math.Min(anchorRowIndex, rowIndex); i <= Math.Max(anchorRowIndex, rowIndex); ++i)
            {
                _selectedRows.Select(_source.Rows.RowIndexToModelIndex(i));
            }

            _selectedRows.AnchorIndex = anchorModelIndex;
            _rangeAnchor = (columnIndex, rowIndex);

            EndBatchUpdate();
        }

        private bool IsSelected(int columnIndex, int rowIndex)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);
            return _selectedColumns.IsSelected(columnIndex) && _selectedRows.IsSelected(modelIndex);
        }

        private void OnSelectedColumnsSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
        {
            _columnsChanged = true;
        }

        private void OnSelectedRowsSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<TModel> e)
        {
            _rowsChanged = true;
        }
    }
}
