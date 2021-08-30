using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Selection
{
    public class HierarchicalTreeDataGridSelectionModel<T> : TreeSelectionModelBase<T>, ITreeDataGridSelectionModel
        where T : class
    {
        private readonly HierarchicalTreeDataGridSource<T> _source;
        private SelectionModel<IRow<T>> _rowSelection;

        public HierarchicalTreeDataGridSelectionModel(HierarchicalTreeDataGridSource<T> source)
            : base(source.Items)
        {
            _source = source;
            _rowSelection = new((IEnumerable<IRow<T>>)source.Rows);
            _rowSelection.SingleSelect = false;
            _rowSelection.PropertyChanged += OnRowPropertyChanged;
            _rowSelection.SelectionChanged += OnRowSelectionChanged;
        }

        ISelectionModel ITreeDataGridSelectionModel.RowSelection => _rowSelection;

        protected internal override IEnumerable<T>? GetChildren(T node)
        {
            return _source.GetModelChildren(node);
        }

        protected override bool TryGetItemAt(IndexPath index, [NotNullWhen(true)] out T result)
        {
            return _source.TryGetModelAt(index, out result);
        }

        private void OnRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AnchorIndex):
                    AnchorIndex = RowToModelIndex(_rowSelection.AnchorIndex);
                    break;
                case nameof(SelectedIndex):
                    SelectedIndex = RowToModelIndex(_rowSelection.SelectedIndex);
                    break;
            }
        }

        private void OnRowSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<IRow<T>> e)
        {
            //if (_isSyncingSelection)
            //    return;

            //foreach (HierarchicalRow<T> row in e.DeselectedItems)
            //    _modelSelection.Remove(row.ModelIndexPath);
            //foreach (HierarchicalRow<T> row in e.SelectedItems)
            //    _modelSelection.TryAdd(row.ModelIndexPath, row.Model);

            //if (SelectionChanged is object || _untypedSelectionChanged is object)
            //{
            //    var ev = new TreeSelectionModelSelectionChangedEventArgs<T>(
            //        Select(e.DeselectedItems, x => ((HierarchicalRow<T>)x).ModelIndexPath),
            //        Select(e.SelectedItems, x => ((HierarchicalRow<T>)x).ModelIndexPath),
            //        Select(e.DeselectedItems, x => ((HierarchicalRow<T>)x).Model),
            //        Select(e.SelectedItems, x => ((HierarchicalRow<T>)x).Model));
            //    SelectionChanged?.Invoke(this, ev);
            //    _untypedSelectionChanged?.Invoke(this, ev);
            //}
        }

        private T? GetModelAt(IndexPath index)
        {
            if (index == default)
                return default;
            if (_source.TryGetModelAt(index, out var result))
                return result;
            throw new IndexOutOfRangeException();
        }

        private int ModelToRowIndex(IndexPath modelIndex)
        {
            if (modelIndex == default)
                return -1;

            var rows = _source.Rows;

            for (var i = 0; i < rows.Count; ++i)
            {
                var row = (HierarchicalRow<T>)rows[i];
                if (row.ModelIndexPath == modelIndex)
                    return i;
            }

            return -1;
        }

        private IndexPath RowToModelIndex(int rowIndex)
        {
            return rowIndex >= 0 ? ((HierarchicalRow<T>)_source.Rows[rowIndex]).ModelIndexPath : default;
        }
    }
}
