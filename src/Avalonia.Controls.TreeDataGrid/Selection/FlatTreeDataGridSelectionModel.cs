using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Selection
{
    public class FlatTreeDataGridSelectionModel<T> : ITreeDataGridSelectionModel
    {
        private readonly FlatTreeDataGridSource<T> _source;
        private SelectionModel<T> _modelSelection;
        private SelectionModel<IRow<T>> _rowSelection;
        private IndexPathAdapter? _selectedIndexes;
        private bool _isSyncingSelection;

        public FlatTreeDataGridSelectionModel(FlatTreeDataGridSource<T> source)
        {
            _source = source;
            _modelSelection = new SelectionModel<T>(source.Items);
            _modelSelection.SelectionChanged += OnSelectionChanged;
            _modelSelection.SingleSelect = false;
            _rowSelection = new SelectionModel<IRow<T>>((IEnumerable<IRow<T>>)source.Rows);
            _rowSelection.SingleSelect = false;
            _rowSelection.SelectionChanged += OnRowSelectionChanged;
        }

        public bool SingleSelect
        {
            get => _modelSelection.SingleSelect;
            set => _modelSelection.SingleSelect = _rowSelection.SingleSelect = value;
        }

        public IndexPath AnchorIndex
        {
            get => ToIndexPath(_modelSelection.AnchorIndex);
            set
            {
                if (value.GetSize() == 0)
                    _modelSelection.AnchorIndex = -1;
                else if (value.GetSize() == 1)
                    _modelSelection.AnchorIndex = value.GetAt(0);
            }
        }

        public IndexPath SelectedIndex
        {
            get => ToIndexPath(_modelSelection.SelectedIndex);
            set
            {
                if (value.GetSize() == 0)
                    _modelSelection.SelectedIndex = -1;
                else if (value.GetSize() == 1)
                    _modelSelection.SelectedIndex = value.GetAt(0);
            }
        }

        public IReadOnlyList<IndexPath> SelectedIndexes
        {
            get => _selectedIndexes ??= new IndexPathAdapter(_modelSelection.SelectedIndexes);
        }

        public int Count => _modelSelection.Count;

        IEnumerable? ITreeSelectionModel.Source
        {
            get => _modelSelection.Source;
            set => throw new NotSupportedException();
        }

        IReadOnlyList<object?> ITreeSelectionModel.SelectedItems => ((ISelectionModel)_modelSelection).SelectedItems;
        public IReadOnlyList<T> SelectedItems => _modelSelection.SelectedItems;

        object? ITreeSelectionModel.SelectedItem
        {
            get => ((ISelectionModel)_modelSelection).SelectedItem;
            set => ((ISelectionModel)_modelSelection).SelectedItem = value;
        }

        ISelectionModel ITreeDataGridSelectionModel.RowSelection => _rowSelection;

        public event EventHandler<SelectionModelIndexesChangedEventArgs>? IndexesChanged;
        public event EventHandler<TreeSelectionModelSelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? LostSelection;
        public event PropertyChangedEventHandler? PropertyChanged;

        public void BeginBatchUpdate() => _modelSelection.BeginBatchUpdate();
        public void EndBatchUpdate() => _modelSelection.EndBatchUpdate();

        public bool IsSelected(IndexPath index)
        {
            return index.GetSize() == 1 ? _modelSelection.IsSelected(index.GetAt(0)) : false;
        }

        public void Select(IndexPath index)
        {
            if (index.GetSize() == 1)
                _modelSelection.Select(index.GetAt(0));
        }

        public void Deselect(IndexPath index)
        {
            if (index.GetSize() == 1)
                _modelSelection.Deselect(index.GetAt(0));
        }

        public void SelectRange(IndexPath start, IndexPath end)
        {
            if (start.GetSize() == 1 && end.GetSize() == 1)
                _modelSelection.SelectRange(start.GetAt(0), end.GetAt(0));
        }

        public void DeselectRange(IndexPath start, IndexPath end)
        {
            if (start.GetSize() == 1 && end.GetSize() == 1)
                _modelSelection.DeselectRange(start.GetAt(0), end.GetAt(0));
        }

        public void SelectAll() => _modelSelection.SelectAll();
        public void Clear() => _modelSelection.Clear();

        private void OnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<T> e)
        {
            if (_isSyncingSelection)
                return;

            try
            {
                _rowSelection.BeginBatchUpdate();

                foreach (var i in e.DeselectedItems)
                {
                    var index = ModelToRowIndex(i);
                    if (index >= 0)
                        _rowSelection.Deselect(index);
                }

                foreach (var i in e.SelectedItems)
                {
                    var index = ModelToRowIndex(i);
                    if (index >= 0)
                        _rowSelection.Select(index);
                }
            }
            finally
            {
                _isSyncingSelection = true;
                _rowSelection.EndBatchUpdate();
                _isSyncingSelection = false;
            }
        }

        private void OnRowSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<IRow<T>> e)
        {
            if (_isSyncingSelection)
                return;

            try
            {
                _modelSelection.BeginBatchUpdate();

                foreach (var row in e.DeselectedItems)
                {
                    _modelSelection.Deselect(row.ModelIndex);
                }

                foreach (var row in e.SelectedItems)
                {
                    _modelSelection.Select(row.ModelIndex);
                }
            }
            finally
            {
                _isSyncingSelection = true;
                _modelSelection.EndBatchUpdate();
                _isSyncingSelection = false;
            }
        }

        private int ModelToRowIndex(T model)
        {
            var rows = _source.Rows;

            // TODO: We probably need to implement lookup in _source.Rows?
            for (var i = 0; i < rows.Count; ++i)
            {
                if (EqualityComparer<T>.Default.Equals(((IRow<T>)rows[i]).Model, model))
                    return i;
            }

            return -1;
        }

        private static IndexPath ToIndexPath(int index) => index >= 0 ? new IndexPath(index) : default;

        private class IndexPathAdapter : IReadOnlyList<IndexPath>
        {
            private IReadOnlyList<int> _source;
            public IndexPathAdapter(IReadOnlyList<int> source) => _source = source;
            public IndexPath this[int index] => new IndexPath(_source[index]);
            public int Count => _source.Count;

            public IEnumerator<IndexPath> GetEnumerator()
            {
                foreach (var i in _source)
                    yield return new IndexPath(i);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
