using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace Avalonia.Controls.Selection
{
    internal class TreeSelectedItemsBase<T> : IReadOnlyList<T?>
    {
        protected readonly TreeSelectionModelBase<T> _owner;

        public TreeSelectedItemsBase(TreeSelectionModelBase<T> owner) => _owner = owner;

        public int Count
        {
            get
            {
                if (_owner.SingleSelect)
                {
                    return _owner.SelectedIndex.Count > 0 ? 1 : 0;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public T this[int index]
        {
            get
            {
                if (index >= Count)
                {
                    throw new IndexOutOfRangeException("The index was out of range.");
                }

                throw new NotImplementedException();
            }
        }

        public IEnumerator<T?> GetEnumerator()
        {
            if (_owner.SingleSelect)
            {
                if (_owner.SelectedIndex.Count > 0)
                {
                    yield return _owner.SelectedItem;
                }
            }
            else
            {
                foreach (var i in EnumerateNode(_owner.Root))
                    yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<T?> EnumerateNode(TreeSelectionNode<T> node)
        {
            foreach (var range in node.Ranges)
            {
                for (var i = range.Begin; i <= range.End; ++i)
                {
                    yield return node.ItemsView![i];
                }
            }

            if (node.Children is object)
            {
                foreach (var child in node.Children)
                {
                    if (child is object)
                    {
                        foreach (var i in EnumerateNode(child))
                            yield return i;
                    }
                }
            }
        }
    }

    internal class TreeSelectedItems<T> : TreeSelectedItemsBase<T>, IReadOnlyList<object?>
    {
        public TreeSelectedItems(TreeSelectionModelBase<T> root) : base(root) { }
        object? IReadOnlyList<object?>.this[int index] => this[index];

        IEnumerator<object?> IEnumerable<object?>.GetEnumerator()
        {
            if (_owner.SingleSelect)
            {
                if (_owner.SelectedIndex.Count > 0)
                {
                    yield return _owner.SelectedItem;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}