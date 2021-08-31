using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls.Models.TreeDataGrid;

#nullable enable

namespace Avalonia.Controls.Selection
{
    internal class TreeSelectionNode<T> : SelectionNodeBase<T>
    {
        private readonly TreeSelectionModelBase<T> _owner;
        private List<TreeSelectionNode<T>?>? _children;

        public TreeSelectionNode(TreeSelectionModelBase<T> owner)
        {
            _owner = owner;
            RangesEnabled = true;
        }

        public TreeSelectionNode(
            TreeSelectionModelBase<T> owner,
            TreeSelectionNode<T> parent,
            int index)
            : this(owner)
        {
            Path = parent.Path.CloneWithChildIndex(index);
            if (parent.ItemsView is object)
                Source = _owner.GetChildren(parent.ItemsView[index]);
        }

        public IndexPath Path { get; private set; }

        public new IEnumerable? Source
        {
            get => base.Source;
            set => base.Source = value;
        }

        internal IReadOnlyList<TreeSelectionNode<T>?>? Children => _children;

        public void Clear(TreeSelectionModelBase<T>.Operation operation)
        {
            if (Ranges.Count > 0)
            {
                operation.DeselectedRanges ??= new();
                foreach (var range in Ranges)
                    operation.DeselectedRanges.Add(Path, range);
            }

            if (_children is object)
            {
                foreach (var child in _children)
                    child?.Clear(operation);
            }
        }

        public int CommitSelect(IndexRange range) => CommitSelect(range.Begin, range.End);
        public int CommitDeselect(IndexRange range) => CommitDeselect(range.Begin, range.End);
        public TreeSelectionNode<T>? GetChild(int index) => index < _children?.Count ? _children[index] : null;

        public TreeSelectionNode<T>? GetOrCreateChild(int index)
        {
            if (GetChild(index) is TreeSelectionNode<T> result)
                return result;

            var childCount = ItemsView is object ? ItemsView.Count : Math.Max(_children?.Count ?? 0, index);

            if (index < childCount)
            {
                _children ??= new List<TreeSelectionNode<T>?>();
                Resize(_children, childCount);
                return _children[index] ??= new TreeSelectionNode<T>(_owner, this, index);
            }

            return null;
        }

        private protected override CollectionChangeState OnItemsAdded(int index, IList items)
        {
            var state = base.OnItemsAdded(index, items);
            var shifted = false;
            
            if (_children is object)
            {
                _children.InsertMany(index, null, items.Count);

                foreach (var child in _children)
                {
                    shifted |= child?.AncestorIndexesChanged(Path, index, items.Count) ?? false;
                }
            }

            if (shifted)
                state.ShiftDelta = shifted ? items.Count : 0;

            return state;
        }

        private bool AncestorIndexesChanged(IndexPath parentIndex, int shiftIndex, int shiftDelta)
        {
            var path = Path;
            var result = false;

            if (ShiftIndex(parentIndex, shiftIndex, shiftDelta, ref path))
            {
                Path = path;
                result = true;
            }

            if (_children is object)
            {
                foreach (var child in _children)
                {
                    result |= child?.AncestorIndexesChanged(parentIndex, shiftIndex, shiftDelta) ?? false;
                }
            }

            return result;
        }

        protected override void OnSourceCollectionChangeFinished()
        {
        }

        private protected override void OnIndexesChanged(int shiftIndex, int shiftDelta)
        {
            _owner.OnIndexesChanged(Path, shiftIndex, shiftDelta);
        }

        private protected override void OnSourceReset()
        {
            throw new NotImplementedException();
        }

        private protected override void OnSelectionChanged(IReadOnlyList<T> deselectedItems)
        {
            throw new NotImplementedException();
        }

        private TreeSelectionNode<T>? GetChild(int index, bool realize)
        {
            if (realize)
            {
                _children ??= new List<TreeSelectionNode<T>?>();

                if (ItemsView is null)
                {
                    if (_children.Count < index + 1)
                    {
                        Resize(_children, index + 1);
                    }

                    return _children[index] ??= new TreeSelectionNode<T>(_owner, this, index);
                }
                else
                {
                    if (_children.Count > ItemsView.Count)
                    {
                        throw new Exception("!!!");
                    }

                    Resize(_children, ItemsView.Count);
                    return _children[index] ??= new TreeSelectionNode<T>(_owner, this, index);
                }
            }
            else
            {
                if (_children?.Count > index)
                {
                    return _children[index];
                }
            }

            return null;
        }

        private static void Resize(List<TreeSelectionNode<T>?> list, int count)
        {
            int current = list.Count;

            if (count < current)
            {
                list.RemoveRange(count, current - count);
            }
            else if (count > current)
            {
                if (count > list.Capacity)
                {
                    list.Capacity = count;
                }

                list.InsertMany(0, null, count - current);
            }
        }

        internal static bool ShiftIndex(IndexPath parentIndex, int shiftIndex, int shiftDelta, ref IndexPath path)
        {
            if (path.GetAt(parentIndex.GetSize()) >= shiftIndex)
            {
                var indexes = path.ToArray();
                indexes[parentIndex.GetSize()] += shiftDelta;
                path = new IndexPath(indexes);
                return true;
            }

            return false;
        }
    }
}
