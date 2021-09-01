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
                    return _owner.SelectedIndex.Count > 0 ? 1 : 0;
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
                if (_owner.SelectedIndex.Count != default)
                {
                    yield return _owner.SelectedIndex;
                }
            }
            else
            {
                foreach (var i in EnumerateNode(_owner.Root))
                    yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<IndexPath> EnumerateNode(TreeSelectionNode<T> node)
        {
            foreach (var range in node.Ranges)
            {
                for (var i = range.Begin; i <= range.End; ++i)
                {
                    yield return node.Path.Append(i);
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
}
