using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace Avalonia.Controls.Selection
{
    internal class TreeSelectedIndexes<T> : IReadOnlyList<IndexPath>
    {
        private readonly TreeSelectionModelBase<T> _owner;

        public TreeSelectedIndexes(TreeSelectionModelBase<T> owner) => _owner = owner;

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

        public IndexPath this[int index]
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

        public IEnumerator<IndexPath> GetEnumerator()
        {
            if (_owner.SingleSelect)
            {
                if (_owner.SelectedIndex.GetSize() > 0)
                {
                    yield return _owner.SelectedIndex;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
