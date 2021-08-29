using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace Avalonia.Controls.Selection
{
    internal class TreeSelectedItemsBase<T> : IReadOnlyList<T?>
    {
        protected readonly TreeSelectionModelBase<T> _model;

        public TreeSelectedItemsBase(TreeSelectionModelBase<T> root) => _model = root;

        public int Count
        {
            get
            {
                if (_model.SingleSelect)
                {
                    return _model.SelectedIndex.GetSize() > 0 ? 1 : 0;
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
            if (_model.SingleSelect)
            {
                if (_model.SelectedIndex.GetSize() > 0)
                {
                    yield return _model.SelectedItem;
                }
            }
            else
            {
                throw new NotImplementedException();
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
            if (_model.SingleSelect)
            {
                if (_model.SelectedIndex.GetSize() > 0)
                {
                    yield return _model.SelectedItem;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}