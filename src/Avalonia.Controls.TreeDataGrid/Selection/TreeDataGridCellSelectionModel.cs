using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Selection
{
    public class TreeDataGridCellSelectionModel<TModel> : ITreeDataGridCellSelectionModel<TModel>,
        ITreeDataGridSelectionInteraction
        where TModel : class
    {
        private EventHandler? _viewSelectionChanged;

        public TreeDataGridCellSelectionModel(ITreeDataGridSource<TModel> source)
        {
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
    }
}
