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

        public IndexPath Path { get; }

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
                CommitDeselect(0, int.MaxValue);
            }

            if (_children is object)
            {
                foreach (var child in _children)
                    child?.Clear(operation);
            }
        }

        public int Select(int index, TreeSelectionModelBase<T>.Operation operation)
        {
            var count = CommitSelect(index, index);

            if (count > 0)
            {
                operation.SelectedRanges ??= new();
                operation.SelectedRanges.Add(Path, new IndexRange(index, index));
            }

            return count;
        }

        public int Deselect(int index, TreeSelectionModelBase<T>.Operation operation)
        {
            var count = CommitDeselect(index, index);

            if (count > 0)
            {
                operation.DeselectedRanges ??= new();
                operation.DeselectedRanges.Add(Path, new IndexRange(index, index));
            }

            return count;
        }

        public bool TryGetNode(
            IndexPath path,
            int depth,
            bool realize,
            [NotNullWhen(true)] out TreeSelectionNode<T>? result)
        {
            if (depth == path.GetSize())
            {
                result = this;
                return true;
            }

            var index = path.GetAt(depth);
            result = GetChild(index, realize);
            return result is object;
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
    }
}
