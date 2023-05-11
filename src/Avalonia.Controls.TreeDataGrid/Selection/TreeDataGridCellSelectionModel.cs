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
        private readonly ITreeDataGridSource<TModel> _source;
        private EventHandler? _viewSelectionChanged;
        private Point _pressedPoint = s_InvalidPoint;

        public TreeDataGridCellSelectionModel(ITreeDataGridSource<TModel> source)
        {
            _source = source;
            SelectedCells = Array.Empty<ICell>();
            SelectedColumns = new TreeDataGridColumnSelectionModel(source.Columns);
            SelectedRows = new TreeDataGridRowSelectionModel<TModel>(source);
        }

        public int Count => SelectedColumns.Count * SelectedRows.Count;

        public bool SingleSelect
        {
            get => SelectedRows.SingleSelect;
            set => SelectedColumns.SingleSelect = SelectedRows.SingleSelect = value;
        }

        public IReadOnlyList<ICell> SelectedCells { get; }
        public ITreeDataGridColumnSelectionModel SelectedColumns { get; }
        public ITreeDataGridRowSelectionModel<TModel> SelectedRows { get; }

        IEnumerable? ITreeDataGridSelection.Source
        {
            get => ((ITreeDataGridSelection)SelectedRows).Source;
            set => ((ITreeDataGridSelection)SelectedRows).Source = value;
        }

        event EventHandler? ITreeDataGridSelectionInteraction.SelectionChanged
        {
            add => _viewSelectionChanged += value;
            remove => _viewSelectionChanged -= value;
        }

        private bool IsSelected(int columnIndex, IndexPath rowIndex)
        {
            return SelectedColumns.IsSelected(columnIndex) && SelectedRows.IsSelected(rowIndex);
        }

        public void Select(int columnIndex, IndexPath rowIndex)
        {
            SelectedColumns.SelectedIndex = columnIndex;
            SelectedRows.SelectedIndex = rowIndex;
            _viewSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        bool ITreeDataGridSelectionInteraction.IsCellSelected(int columnIndex, int rowIndex)
        {
            return IsSelected(columnIndex, rowIndex);
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
                    Select(columnIndex, modelIndex);
            }
            else if (range)
            {
                if (!treeDataGrid.QueryCancelSelection())
                    SelectFromAnchorTo(columnIndex, rowIndex);
            }
            else if (SelectedColumns.SelectedIndex != columnIndex || 
                SelectedRows.SelectedIndex != modelIndex ||
                Count > 1)
            {
                if (!treeDataGrid.QueryCancelSelection())
                    Select(columnIndex, modelIndex);
            }
        }

        private void SelectFromAnchorTo(int columnIndex, int rowIndex)
        {
            var anchorColumnIndex = SelectedColumns.AnchorIndex;
            var anchorModelIndex = SelectedRows.AnchorIndex;
            var anchorRowIndex = _source.Rows.ModelIndexToRowIndex(anchorModelIndex);

            SelectedColumns.BeginBatchUpdate();
            SelectedColumns.Clear();
            SelectedColumns.SelectRange(anchorColumnIndex, columnIndex);
            SelectedColumns.EndBatchUpdate();

            SelectedRows.BeginBatchUpdate();
            SelectedRows.Clear();
            for (var i = Math.Min(anchorRowIndex, rowIndex); i <= Math.Max(anchorRowIndex, rowIndex); ++i)
            {
                SelectedRows.Select(_source.Rows.RowIndexToModelIndex(i));
            }
            SelectedRows.AnchorIndex = anchorRowIndex;
            SelectedRows.EndBatchUpdate();

            _viewSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool IsSelected(int columnIndex, int rowIndex)
        {
            var modelIndex = _source.Rows.RowIndexToModelIndex(rowIndex);
            return SelectedColumns.IsSelected(columnIndex) && SelectedRows.IsSelected(modelIndex);
        }
    }
}
