using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Selection;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class HierarchicalSelectionModel<TModel> : NotifyingBase, ISelectionModel
        where TModel : class
    {
        private readonly SelectionModel<HierarchicalRow<TModel>> _rowSelection;
        private IndexSelector? _selectedIndexes;
        private ModelSelector? _selectedItems;
        private EventHandler<SelectionModelSelectionChangedEventArgs>? _untypedSelectedChanged;

        public HierarchicalSelectionModel(HierarchicalTreeDataGridSource<TModel> source)
        {
            _rowSelection = new SelectionModel<HierarchicalRow<TModel>>(
                (HierarchicalRows<TModel>)source.Rows);
            _rowSelection.SelectionChanged += OnRowSelectionChanged;
        }

        public bool SingleSelect
        {
            get => _rowSelection.SingleSelect;
            set => _rowSelection.SingleSelect = value;
        }

        public IndexPath SelectedIndex => _rowSelection.SelectedItem?.ModelIndexPath ?? default;
        
        public IReadOnlyList<IndexPath> SelectedIndexes =>
            _selectedIndexes ??= new IndexSelector(_rowSelection.SelectedItems);

        [MaybeNull]
        public TModel SelectedItem => _rowSelection.SelectedItem?.Model;

        public IReadOnlyList<TModel> SelectedItems =>
            _selectedItems ??= new ModelSelector(_rowSelection.SelectedItems);

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

        void ISelectionModel.Deselect(int index) => _rowSelection.Deselect(index);
        void ISelectionModel.DeselectRange(int start, int end) => _rowSelection.DeselectRange(start, end);
        bool ISelectionModel.IsSelected(int index) => _rowSelection.IsSelected(index);
        void ISelectionModel.Select(int index) => _rowSelection.Select(index);
        void ISelectionModel.SelectRange(int start, int end) => _rowSelection.SelectRange(start, end);

        private void OnRowSelectionChanged(
            object sender,
            SelectionModelSelectionChangedEventArgs<HierarchicalRow<TModel>> e)
        {
            _untypedSelectedChanged?.Invoke(this, e);

            SelectionChanged?.Invoke(
                this,
                new TreeSelectionModelSelectionChangedEventArgs<TModel>(
                    new IndexSelector(e.DeselectedItems),
                    new IndexSelector(e.SelectedItems),
                    new ModelSelector(e.DeselectedItems),
                    new ModelSelector(e.SelectedItems)));
        }

        private class IndexSelector : IReadOnlyList<IndexPath>
        {
            private IReadOnlyList<HierarchicalRow<TModel>> _inner;
            public IndexSelector(IReadOnlyList<HierarchicalRow<TModel>> inner) => _inner = inner;
            public IndexPath this[int index] => _inner[index].ModelIndexPath;
            public int Count => _inner.Count;

            public IEnumerator<IndexPath> GetEnumerator()
            {
                foreach (var i in _inner)
                    yield return i.ModelIndexPath;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class ModelSelector : IReadOnlyList<TModel>
        {
            private IReadOnlyList<HierarchicalRow<TModel>> _inner;
            public ModelSelector(IReadOnlyList<HierarchicalRow<TModel>> inner) => _inner = inner;
            public TModel this[int index] => _inner[index].Model;
            public int Count => _inner.Count;

            public IEnumerator<TModel> GetEnumerator()
            {
                foreach (var i in _inner)
                    yield return i.Model;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
