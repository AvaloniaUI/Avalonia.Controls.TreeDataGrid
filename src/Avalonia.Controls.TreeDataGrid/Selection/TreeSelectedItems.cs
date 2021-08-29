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
                    return _owner.SelectedIndex.GetSize() > 0 ? 1 : 0;
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
                if (_owner.SelectedIndex.GetSize() > 0)
                {
                    yield return _owner.SelectedItem;
                }
            }
            else
            {
                var node = _owner.Root;

                foreach (var range in node.Ranges)
                {
                    for (var i = range.Begin; i <= range.End; ++i)
                    {
                        yield return node.ItemsView is object ? node.ItemsView[i] : default;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class TreeSelectedItems<T> : TreeSelectedItemsBase<T>, IReadOnlyList<object?>
    {
        public TreeSelectedItems(TreeSelectionModelBase<T> root) : base(root) { }
        object? IReadOnlyList<object?>.this[int index] => this[index];

        IEnumerator<object?> IEnumerable<object?>.GetEnumerator()
        {
            if (_owner.SingleSelect)
            {
                if (_owner.SelectedIndex.GetSize() > 0)
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