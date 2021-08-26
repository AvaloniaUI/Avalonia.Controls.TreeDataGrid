using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
            set => throw new NotImplementedException();
        }

        public IReadOnlyList<IndexPath> SelectedIndexes
        {
            get => _selectedIndexes ??= new(_modelSelection.Keys);
        }

        public int Count => _modelSelection.Count;

        public T SelectedItem => GetModelAt(_selectedIndex);

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
        public event EventHandler<TreeSelectionModelSelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler? LostSelection;
        public event PropertyChangedEventHandler? PropertyChanged;

        public void BeginBatchUpdate() => _rowSelection.BeginBatchUpdate();
        public void EndBatchUpdate() => _rowSelection.EndBatchUpdate();
        public bool IsSelected(IndexPath index) => _modelSelection.ContainsKey(index);
        
        public void Select(IndexPath index)
        {
            if (!_modelSelection.ContainsKey(index) && _source.TryGetModelAt(index, out var model))
            {
                _modelSelection.Add(index, model);
            }
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
            foreach (HierarchicalRow<T> row in e.DeselectedItems)
                _modelSelection.Remove(row.ModelIndexPath);
            foreach (HierarchicalRow<T> row in e.SelectedItems)
                _modelSelection.Add(row.ModelIndexPath, row.Model);

            if (SelectionChanged is object)
            {
                static IReadOnlyList<TDest> RowAdapter<TDest>(
                    IReadOnlyList<IRow<T>> source,
                    Func<IRow<T>, TDest> selector)
                {
                    return new ReadOnlyListAdapter<IRow<T>, TDest>(source, selector);
                }

                var ev = new TreeSelectionModelSelectionChangedEventArgs<T>(
                    RowAdapter(e.DeselectedItems, x => ((HierarchicalRow<T>)x).ModelIndexPath),
                    RowAdapter(e.SelectedItems, x => ((HierarchicalRow<T>)x).ModelIndexPath),
                    RowAdapter(e.DeselectedItems, x => ((HierarchicalRow<T>)x).Model),
                    RowAdapter(e.SelectedItems, x => ((HierarchicalRow<T>)x).Model));
            }
        }

        private T GetModelAt(IndexPath index)
        {
            if (_source.TryGetModelAt(index, out var result))
                return result;
            else
                throw new IndexOutOfRangeException();
        }

        private HierarchicalRow<T>? GetRowFor(IndexPath modelIndex)
        {
            foreach (HierarchicalRow<T> row in _source.Rows)
            {
                if (row.ModelIndexPath == modelIndex)
                    return row;
            }

            return null;
        }

        private IndexPath RowToModelIndex(int rowIndex)
        {
            return rowIndex >= 0 ? ((HierarchicalRow<T>)_source.Rows[rowIndex]).ModelIndexPath : default;
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
