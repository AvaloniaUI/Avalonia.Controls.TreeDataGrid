using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
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

        public void Select(int columnIndex, IndexPath rowIndex)
        {
            SelectedColumns.Select(columnIndex);
            SelectedRows.Select(rowIndex);
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
            //
            // Otherwise select on pointer release.
            if (!e.Handled &&
                e.Pointer.Type == PointerType.Mouse &&
                e.Source is Control source &&
                sender.TryGetCell(source, out var cell) &&
                _source.Rows.RowIndexToModelIndex(cell.RowIndex) is { } modelIndex)
            {
                Select(cell.ColumnIndex, modelIndex);
            }
            else
            {
                _pressedPoint = e.GetPosition(sender);
            }
        }

        private bool IsSelected(int columnIndex, int rowIndex)
        {
            if (_source.Rows.RowIndexToModelIndex(rowIndex) is { } modelIndex)
                return SelectedColumns.IsSelected(columnIndex) && SelectedRows.IsSelected(rowIndex);
            return false;
        }
    }
}
