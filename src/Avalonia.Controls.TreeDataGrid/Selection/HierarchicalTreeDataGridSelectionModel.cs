using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Selection
{
    public class HierarchicalTreeDataGridSelectionModel<T> : ITreeDataGridSelectionModel
        where T : class
    {
        private readonly HierarchicalTreeDataGridSource<T> _source;
        private SortedList<IndexPath, T> _modelSelection;
        private SelectionModel<IRow<T>> _rowSelection;
        private ReadOnlyListAdapter<IndexPath>? _selectedIndexes;
        private ReadOnlyListAdapter<T>? _selectedItems;
        private bool _singleSelect;
        private IndexPath _anchorIndex;
        private IndexPath _selectedIndex;
        private EventHandler<TreeSelectionModelSelectionChangedEventArgs>? _untypedSelectionChanged;
        private bool _isSyncingSelection;

        public HierarchicalTreeDataGridSelectionModel(HierarchicalTreeDataGridSource<T> source)
        {
            _source = source;
            _modelSelection = new();
            _rowSelection = new((IEnumerable<IRow<T>>)source.Rows);
            _rowSelection.SingleSelect = false;
            _rowSelection.PropertyChanged += OnRowPropertyChanged;
            _rowSelection.SelectionChanged += OnRowSelectionChanged;
        }

        public bool SingleSelect
        {
            get => _singleSelect;
            set
            {
                if (_singleSelect != value)
                {
                    _singleSelect = value;
                    RaisePropertyChanged(nameof(SingleSelect));
                }
            }
        }

        public IndexPath AnchorIndex
        {
            get => _anchorIndex;
            set
            {
                if (_anchorIndex != value)
                {
                    _anchorIndex = value;
                    RaisePropertyChanged(nameof(AnchorIndex));
                }
            }
        }

        public IndexPath SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value && _modelSelection.Count == 1)
                    return;

                var oldSelection = _modelSelection;
                _modelSelection = new();
                _selectedIndex = value;

                if (_source.TryGetModelAt(value, out var model))
                    _modelSelection.Add(value, model);
                SyncRowSelection();
                RaiseSelectionChanged(oldSelection);
            }
        }

        public IReadOnlyList<IndexPath> SelectedIndexes
        {
            get => _selectedIndexes ??= new(_modelSelection.Keys);
        }

        public int Count => _modelSelection.Count;

        public T? SelectedItem => GetModelAt(_selectedIndex);

        public IReadOnlyList<T> SelectedItems
        {
            get => _selectedItems ??= new(_modelSelection.Values);
        }

        object? ITreeSelectionModel.SelectedItem => SelectedItem;
        IReadOnlyList<object?> ITreeSelectionModel.SelectedItems => SelectedItems;
        ISelectionModel ITreeDataGridSelectionModel.RowSelection => _rowSelection;

        IEnumerable? ITreeSelectionModel.Source
        {
            get => _source.Items;
            set => throw new NotSupportedException();
        }

        public event EventHandler<SelectionModelIndexesChangedEventArgs>? IndexesChanged;
        public event EventHandler<TreeSelectionModelSelectionChangedEventArgs<T>>? SelectionChanged;
        public event EventHandler? LostSelection;
        public event PropertyChangedEventHandler? PropertyChanged;

        event EventHandler<TreeSelectionModelSelectionChangedEventArgs>? ITreeSelectionModel.SelectionChanged
        {
            add => _untypedSelectionChanged += value;
            remove => _untypedSelectionChanged -= value;
        }

        public void BeginBatchUpdate() => _rowSelection.BeginBatchUpdate();
        public void EndBatchUpdate() => _rowSelection.EndBatchUpdate();
        public bool IsSelected(IndexPath index) => _modelSelection.ContainsKey(index);
        
        public void Select(IndexPath index)
        {
            if (SingleSelect)
            {
                if (!IsSelected(index) && _source.TryGetModelAt(index, out var model))
                {
                    var oldSelection = _modelSelection;
                    _modelSelection = new();
                    _selectedIndex = index;
                    _modelSelection.Add(index, model);
                    SyncRowSelection();
                    RaiseSelectionChanged(oldSelection);
                }
            }
            else
                throw new NotImplementedException();
        }
        
        public void Deselect(IndexPath index) => throw new NotImplementedException();
        public void SelectRange(IndexPath start, IndexPath end) => throw new NotImplementedException();
        public void DeselectRange(IndexPath start, IndexPath end) => throw new NotImplementedException();
        public void SelectAll() => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SyncRowSelection()
        {
            _isSyncingSelection = true;

            try
            {
                if (_modelSelection.Count == 0)
                    _rowSelection.Clear();
                if (_modelSelection.Count == 1)
                    _rowSelection.SelectedIndex = ModelToRowIndex(_modelSelection.Keys[0]);
            }
            finally
            {
                _isSyncingSelection = false;
            }
        }

        private void OnRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AnchorIndex):
                    _anchorIndex = RowToModelIndex(_rowSelection.AnchorIndex);
                    break;
                case nameof(SelectedIndex):
                    _selectedIndex = RowToModelIndex(_rowSelection.SelectedIndex);
                    break;
            }
        }

        private void OnRowSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<IRow<T>> e)
        {
            if (_isSyncingSelection)
                return;

            foreach (HierarchicalRow<T> row in e.DeselectedItems)
                _modelSelection.Remove(row.ModelIndexPath);
            foreach (HierarchicalRow<T> row in e.SelectedItems)
                _modelSelection.TryAdd(row.ModelIndexPath, row.Model);

            if (SelectionChanged is object || _untypedSelectionChanged is object)
            {
                var ev = new TreeSelectionModelSelectionChangedEventArgs<T>(
                    Select(e.DeselectedItems, x => ((HierarchicalRow<T>)x).ModelIndexPath),
                    Select(e.SelectedItems, x => ((HierarchicalRow<T>)x).ModelIndexPath),
                    Select(e.DeselectedItems, x => ((HierarchicalRow<T>)x).Model),
                    Select(e.SelectedItems, x => ((HierarchicalRow<T>)x).Model));
                SelectionChanged?.Invoke(this, ev);
                _untypedSelectionChanged?.Invoke(this, ev);
            }
        }

        private void RaiseSelectionChanged(SortedList<IndexPath, T> oldSelection)
        {
            if (SelectionChanged is object || _untypedSelectionChanged is object)
            {
                var deselected = oldSelection.Except(_modelSelection).ToList();
                var selected = _modelSelection.Except(oldSelection).ToList();

                var ev = new TreeSelectionModelSelectionChangedEventArgs<T>(
                    Select(deselected, x => x.Key),
                    Select(selected, x => x.Key),
                    Select(deselected, x => x.Value),
                    Select(selected, x => x.Value));
                SelectionChanged?.Invoke(this, ev);
                _untypedSelectionChanged?.Invoke(this, ev);
            }
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

        private static IReadOnlyList<TDest> Select<TSource, TDest>(
            IReadOnlyList<TSource> source,
            Func<TSource, TDest> selector)
        {
            return new ReadOnlyListAdapter<TSource, TDest>(source, selector);
        }

        private class ReadOnlyListAdapter<TItem> : IReadOnlyList<TItem>
        {
            private readonly IList<TItem> _inner;
            public ReadOnlyListAdapter(IList<TItem> inner) => _inner = inner;
            public TItem this[int index] => _inner[index];
            public int Count => _inner.Count;
            public IEnumerator<TItem> GetEnumerator() => _inner.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
        }

        private class ReadOnlyListAdapter<TSource, TDest> : IReadOnlyList<TDest>
        {
            private readonly IReadOnlyList<TSource> _inner;
            private readonly Func<TSource, TDest> _selector;
            
            public ReadOnlyListAdapter(IReadOnlyList<TSource> inner, Func<TSource, TDest> selector)
            {
                _inner = inner;
                _selector = selector;
            }

            public TDest this[int index] => _selector(_inner[index]);
            public int Count => _inner.Count;
            public IEnumerator<TDest> GetEnumerator()
            {
                foreach (var i in _inner)
                    yield return _selector(i);
            }

            IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
        }
    }
}
