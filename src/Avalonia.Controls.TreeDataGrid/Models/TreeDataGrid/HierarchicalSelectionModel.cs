using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls.Selection;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class HierarchicalSelectionModel<TModel> : NotifyingBase, ISelectionModel
        where TModel : class
    {
        private readonly HierarchicalTreeDataGridSource<TModel> _source;
        private readonly SelectionModel<HierarchicalRow<TModel>> _rowSelection;
        private readonly IndexRanges _selectedIndexes;
        private ModelView? _selectedItems;
        private EventHandler<SelectionModelSelectionChangedEventArgs>? _untypedSelectedChanged;
        private int _rowExpandingCollapsing;

        public HierarchicalSelectionModel(HierarchicalTreeDataGridSource<TModel> source)
        {
            _source = source;
            _source.RowExpanding += OnRowExpanding;
            _source.RowExpanded += OnRowExpanded;
            _source.RowCollapsing += OnRowCollapsing;
            _source.RowCollapsed += OnRowCollapsed;
            _rowSelection = new SelectionModel<HierarchicalRow<TModel>>(
                (HierarchicalRows<TModel>)source.Rows);
            _rowSelection.SelectionChanged += OnRowSelectionChanged;
            _selectedIndexes = new IndexRanges();
        }

        public bool SingleSelect
        {
            get => _rowSelection.SingleSelect;
            set => _rowSelection.SingleSelect = value;
        }

        public IndexPath SelectedIndex => _rowSelection.SelectedItem?.ModelIndexPath ?? default;
        public IReadOnlyList<IndexPath> SelectedIndexes => _selectedIndexes;

        [MaybeNull]
        public TModel SelectedItem => _rowSelection.SelectedItem?.Model;
        public IReadOnlyList<TModel> SelectedItems => _selectedItems ??= new ModelView(this, _selectedIndexes);

        IEnumerable? ISelectionModel.Source 
        { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }

        int ISelectionModel.SelectedIndex 
        {
            get => _rowSelection.SelectedIndex;
            set => _rowSelection.SelectedIndex = value; 
        }

        IReadOnlyList<int> ISelectionModel.SelectedIndexes => _rowSelection.SelectedIndexes;

        object? ISelectionModel.SelectedItem 
        { 
            get => _rowSelection.SelectedItem; 
            set => _rowSelection.SelectedItem = (HierarchicalRow<TModel>?)value; 
        }

        IReadOnlyList<object?> ISelectionModel.SelectedItems => _rowSelection.SelectedItems;

        int ISelectionModel.AnchorIndex 
        {
            get => _rowSelection.AnchorIndex;
            set => _rowSelection.AnchorIndex = value; 
        }

        int ISelectionModel.Count => _rowSelection.Count;

        public event EventHandler<TreeSelectionModelSelectionChangedEventArgs<TModel>>? SelectionChanged;

        event EventHandler<SelectionModelIndexesChangedEventArgs>? ISelectionModel.IndexesChanged
        {
            add => _rowSelection.IndexesChanged += value;
            remove => _rowSelection.IndexesChanged -= value;
        }

        event EventHandler<SelectionModelSelectionChangedEventArgs>? ISelectionModel.SelectionChanged
        {
            add => _untypedSelectedChanged += value;
            remove => _untypedSelectedChanged -= value;
        }

        event EventHandler? ISelectionModel.LostSelection
        {
            add => _rowSelection.LostSelection += value;
            remove => _rowSelection.LostSelection -= value;
        }

        event EventHandler? ISelectionModel.SourceReset
        {
            add => _rowSelection.SourceReset += value;
            remove => _rowSelection.SourceReset -= value;
        }

        public void BeginBatchUpdate() => _rowSelection.BeginBatchUpdate();
        public void EndBatchUpdate() => _rowSelection.EndBatchUpdate();
        public void Clear() => _rowSelection.Clear();
        public void SelectAll() => _rowSelection.SelectAll();
        public bool IsSelected(in IndexPath index) => _selectedIndexes.Contains(index);

        void ISelectionModel.Deselect(int index) => _rowSelection.Deselect(index);
        void ISelectionModel.DeselectRange(int start, int end) => _rowSelection.DeselectRange(start, end);
        bool ISelectionModel.IsSelected(int index) => _rowSelection.IsSelected(index);
        void ISelectionModel.Select(int index) => _rowSelection.Select(index);
        void ISelectionModel.SelectRange(int start, int end) => _rowSelection.SelectRange(start, end);

        private void UpdateExpandedSelection(HierarchicalRow<TModel> row, int parentRowIndex)
        {
            if (row.Children is null || !_selectedIndexes.ContainsDescendents(row.ModelIndexPath))
                return;

            var rowIndex = parentRowIndex + 1;

            for (var i = 0; i < row.Children.Count; ++i)
            {
                var child = row.Children[i];

                if (IsSelected(child.ModelIndexPath))
                {
                    _rowSelection.Select(rowIndex);
                }

                UpdateExpandedSelection(child, rowIndex);
                rowIndex += (child.Children?.Count ?? 0) + 1;
            }
        }

        private void OnRowExpanding(object sender, RowEventArgs<HierarchicalRow<TModel>> e)
        {
            ++_rowExpandingCollapsing;
        }
        
        private void OnRowExpanded(object sender, RowEventArgs<HierarchicalRow<TModel>> e)
        {
            if (_selectedIndexes.Count > 0 &&
                e.Row.Children?.Count > 0 &&
                _selectedIndexes.ContainsDescendents(e.Row.ModelIndexPath))
            {
                using var update = _rowSelection.BatchUpdate();
                var parentRowIndex = _source.GetRowIndex(e.Row.ModelIndexPath);
                UpdateExpandedSelection(e.Row, parentRowIndex);
            }

            --_rowExpandingCollapsing;
        }

        private void OnRowCollapsing(object sender, RowEventArgs<HierarchicalRow<TModel>> e)
        {
            ++_rowExpandingCollapsing;
        }

        private void OnRowCollapsed(object sender, RowEventArgs<HierarchicalRow<TModel>> e)
        {
            --_rowExpandingCollapsing;
        }

        private void OnRowSelectionChanged(
            object sender,
            SelectionModelSelectionChangedEventArgs<HierarchicalRow<TModel>> e)
        {
            static SelectionView<IndexPath> IndexSelector(IReadOnlyList<HierarchicalRow<TModel>> rows)
            {
                return new SelectionView<IndexPath>(rows, x => x.ModelIndexPath);
            }

            static SelectionView<TModel> ModelSelector(IReadOnlyList<HierarchicalRow<TModel>> rows)
            {
                return new SelectionView<TModel>(rows, x => x.Model);
            }

            if (_rowExpandingCollapsing > 0)
            {
                _untypedSelectedChanged?.Invoke(this, e);
                return;
            }

            foreach (var row in e.SelectedItems)
            {
                _selectedIndexes.Add(row.ModelIndexPath);
            }

            foreach (var row in e.DeselectedItems)
            {
                _selectedIndexes.Remove(row.ModelIndexPath);
            }

            _untypedSelectedChanged?.Invoke(this, e);

            SelectionChanged?.Invoke(
                this,
                new TreeSelectionModelSelectionChangedEventArgs<TModel>(
                    IndexSelector(e.DeselectedItems),
                    IndexSelector(e.SelectedItems),
                    ModelSelector(e.DeselectedItems),
                    ModelSelector(e.SelectedItems)));
        }

        private class ModelView : IReadOnlyList<TModel>
        {
            private readonly HierarchicalSelectionModel<TModel> _owner;
            private readonly IndexRanges _ranges;

            public ModelView(HierarchicalSelectionModel<TModel> owner, IndexRanges ranges)
            {
                _owner = owner;
                _ranges = ranges;
            }

            public TModel this[int index] => _owner._source.GetModelAt(_ranges[index]);
            public int Count => _ranges.Count;

            public IEnumerator<TModel> GetEnumerator()
            {
                foreach (var i in _ranges)
                    yield return _owner._source.GetModelAt(i);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class SelectionView<T> : IReadOnlyList<T>
        {
            private readonly IReadOnlyList<HierarchicalRow<TModel>> _items;
            private readonly Func<HierarchicalRow<TModel>, T> _selector;

            public SelectionView(
                IReadOnlyList<HierarchicalRow<TModel>> items,
                Func<HierarchicalRow<TModel>, T> selector)
            {
                _items = items;
                _selector = selector;
            }

            public T this[int index] => _selector(_items[index]);
            public int Count => _items.Count;

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var row in _items)
                    yield return _selector(row);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
