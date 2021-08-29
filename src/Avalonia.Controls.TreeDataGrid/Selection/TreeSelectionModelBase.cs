using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Avalonia.Controls.Selection
{
    public abstract class TreeSelectionModelBase<T> : ITreeSelectionModel, INotifyPropertyChanged
    {
        private TreeSelectionNode<T> _root;
        private IndexPath _anchorIndex;
        private IndexPath _selectedIndex;
        private Operation? _operation;
        private TreeSelectedIndexes<T>? _selectedIndexes;
        private TreeSelectedItems<T>? _selectedItems;

        protected TreeSelectionModelBase()
        {
            _root = new(this);
        }

        protected TreeSelectionModelBase(IEnumerable source)
            : this()
        {
            Source = source;
        }

        public int Count { get; }

        public bool SingleSelect { get; set; } = true;

        public IndexPath SelectedIndex 
        {
            get => _selectedIndex;
            set
            {
                using var update = BatchUpdate();
                Clear();
                Select(value);
            }
        }

        public IReadOnlyList<IndexPath> SelectedIndexes => _selectedIndexes ??= new(this);
        public T? SelectedItem => GetItemAt(_selectedIndex);

        public IReadOnlyList<T?> SelectedItems => _selectedItems ??= new(this);

        public IndexPath AnchorIndex 
        {
            get => _anchorIndex;
            set => _anchorIndex = value;
        }

        object? ITreeSelectionModel.SelectedItem => SelectedItem;
        IReadOnlyList<object?> ITreeSelectionModel.SelectedItems => _selectedItems ??= new(this);

        IEnumerable? ITreeSelectionModel.Source
        {
            get => Source;
            set => Source = (IEnumerable<T>?)value;
        }

        protected IEnumerable? Source 
        {
            get => _root.Source;
            set => _root.Source = value;
        }

        public event EventHandler<TreeSelectionModelSelectionChangedEventArgs<T>>? SelectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<SelectionModelIndexesChangedEventArgs>? IndexesChanged;
        public event EventHandler? LostSelection;

        event EventHandler<TreeSelectionModelSelectionChangedEventArgs>? ITreeSelectionModel.SelectionChanged
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public BatchUpdateOperation BatchUpdate() => new BatchUpdateOperation(this);

        public void BeginBatchUpdate()
        {
            _operation ??= new Operation(this);
            ++_operation.UpdateCount;
        }

        public void EndBatchUpdate()
        {
            if (_operation is null || _operation.UpdateCount == 0)
            {
                throw new InvalidOperationException("No batch update in progress.");
            }

            if (--_operation.UpdateCount == 0)
            {
                // If the collection is currently changing, commit the update when the
                // collection change finishes.
                ////if (!IsSourceCollectionChanging)
                {
                    CommitOperation(_operation);
                }
            }
        }
        
        public void Clear() => DeselectRange(new IndexPath(0), new IndexPath(int.MaxValue));

        public void Deselect(IndexPath index)
        {
            throw new NotImplementedException();
        }

        public void DeselectRange(IndexPath start, IndexPath end)
        {
            using var update = BatchUpdate();
            var o = update.Operation;

            _root.Deselect(start, end, o);
        }

        public bool IsSelected(IndexPath index)
        {
            throw new NotImplementedException();
        }

        public void Select(IndexPath index) => SelectRange(index, index, false, true);

        public void SelectAll()
        {
            throw new NotImplementedException();
        }

        public void SelectRange(IndexPath start, IndexPath end)
        {
            throw new NotImplementedException();
        }

        protected internal abstract IEnumerable<T>? GetChildren(T node);

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnIndexesChanged(IndexPath parentPath, int shiftIndex, int shiftDelta)
        {
            if (ShiftIndex(parentPath, shiftIndex, shiftDelta, ref _selectedIndex))
            {
                RaisePropertyChanged(nameof(SelectedIndex));
            }

            if (ShiftIndex(parentPath, shiftIndex, shiftDelta, ref _anchorIndex))
            {
                RaisePropertyChanged(nameof(AnchorIndex));
            }
        }

        private void SelectRange(
            IndexPath start,
            IndexPath end,
            bool forceSelectedIndex,
            bool forceAnchorIndex)
        {
            if (SingleSelect && start != end)
            {
                throw new InvalidOperationException("Cannot select range with single selection.");
            }

            CoerceRange(ref start, ref end);

            if (start == default)
            {
                return;
            }

            using var update = BatchUpdate();
            var o = update.Operation;
            var selected = new List<IndexRange>();

            if (!SingleSelect)
            {
                o.SelectedRanges ??= new();
                o.SelectedRanges.RemoveRange(start, end);
                //IndexRange.Remove(o.DeselectedRanges, range);
                //IndexRange.Add(o.SelectedRanges, range);
                //IndexRange.Remove(o.SelectedRanges, Ranges);

                //if (o.SelectedIndex == -1 || forceSelectedIndex)
                //{
                //    o.SelectedIndex = range.Begin;
                //}

                //if (o.AnchorIndex == -1 || forceAnchorIndex)
                //{
                //    o.AnchorIndex = range.Begin;
                //}
            }
            else
            {
                o.SelectedIndex = o.AnchorIndex = start;
            }

            //_initSelectedItems = null;
        }

        private void CoerceRange(ref IndexPath start, ref IndexPath end)
        {
            start = _root.CoerceIndex(start, 0);
            end = _root.CoerceIndex(end, 0);
        }

        private bool ShiftIndex(IndexPath parentPath, int shiftIndex, int shiftDelta, ref IndexPath path)
        {
            if (parentPath.IsAncestorOf(path) && path.GetAt(parentPath.GetSize()) >= shiftIndex)
            {
                var indexes = path.ToArray();
                ++indexes[parentPath.GetSize()];
                path = new IndexPath(indexes);
                return true;
            }

            return false;
        }

        private T? GetItemAt(in IndexPath path)
        {
            if (Source is null || path == default)
            {
                return default;
            }

            if (path != default)
            {
                var node = GetNode(path.GetParent());

                if (node is object)
                {
                    return node.ItemsView![path.GetLeaf()!.Value];
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        private TreeSelectionNode<T>? GetNode(in IndexPath path)
        {
            if (path == default)
            {
                return _root;
            }

            if (_root.TryGetNode(path, 0, false, out var result))
            {
                return result;
            }

            return null;
        }

        private TreeSelectionNode<T> RealizeNode(in IndexPath path)
        {
            if (path == default)
            {
                return _root;
            }

            if (_root.TryGetNode(path, 0, true, out var result))
            {
                return result;
            }

            throw new ArgumentOutOfRangeException();
        }

        private IndexPath CoerceIndex(IndexPath path)
        {
            if (Source is null)
            {
                return path;
            }

            return _root.CoerceIndex(path, 0);
        }

        private void CommitOperation(Operation operation)
        {
            var oldAnchorIndex = _anchorIndex;
            var oldSelectedIndex = _selectedIndex;

            _selectedIndex = operation.SelectedIndex;
            _anchorIndex = operation.AnchorIndex;

            if (_selectedIndex != oldSelectedIndex)
            {
                if (oldSelectedIndex != default)
                {
                    //CommitDeselect(oldSelectedIndex);
                }

                if (_selectedIndex != default)
                {
                    //CommitSelect(_selectedIndex);
                }
            }

            if (SelectionChanged is object)
            {
                IReadOnlyList<IndexPath>? deselectedIndexes = null;
                IReadOnlyList<IndexPath>? selectedIndexes = null;
                IReadOnlyList<T>? deselectedItems = null;
                IReadOnlyList<T>? selectedItems = null;

                if (SingleSelect && oldSelectedIndex != _selectedIndex)
                {
                    if (oldSelectedIndex != default)
                    {
                        deselectedIndexes = new[] { oldSelectedIndex };
                        deselectedItems = new[] { GetItemAt(oldSelectedIndex)! };
                    }

                    if (_selectedIndex != default)
                    {
                        selectedIndexes = new[] { _selectedIndex };
                        selectedItems = new[] { GetItemAt(_selectedIndex)! };
                    }
                }

                if (deselectedIndexes?.Count > 0 || selectedIndexes?.Count > 0)
                {
                    var e = new TreeSelectionModelSelectionChangedEventArgs<T>(
                        deselectedIndexes,
                        selectedIndexes,
                        deselectedItems,
                        selectedItems);
                    SelectionChanged?.Invoke(this, e);
                }
            }

            if (oldSelectedIndex != _selectedIndex)
            {
                RaisePropertyChanged(nameof(SelectedIndex));
            }

            if (oldAnchorIndex != _anchorIndex)
            {
                RaisePropertyChanged(nameof(AnchorIndex));
            }

            _operation = null;
        }

        public struct BatchUpdateOperation : IDisposable
        {
            private readonly TreeSelectionModelBase<T> _owner;
            private bool _isDisposed;

            public BatchUpdateOperation(TreeSelectionModelBase<T> owner)
            {
                _owner = owner;
                _isDisposed = false;
                owner.BeginBatchUpdate();
            }

            internal Operation Operation => _owner._operation!;

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _owner?.EndBatchUpdate();
                    _isDisposed = true;
                }
            }
        }

        internal class Operation
        {
            public Operation(TreeSelectionModelBase<T> owner)
            {
                AnchorIndex = owner.AnchorIndex;
                SelectedIndex = owner.SelectedIndex;
            }

            public int UpdateCount { get; set; }
            public bool IsSourceUpdate { get; set; }
            public IndexPath AnchorIndex { get; set; }
            public IndexPath SelectedIndex { get; set; }
            public IndexRanges? SelectedRanges { get; set; }
            public IndexRanges? DeselectedRanges { get; set; }
            public IReadOnlyList<T>? DeselectedItems { get; set; }
        }
    }
}
