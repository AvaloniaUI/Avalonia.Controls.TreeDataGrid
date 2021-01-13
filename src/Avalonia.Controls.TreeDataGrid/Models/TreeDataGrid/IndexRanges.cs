using System;
using System.Collections;
using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    internal class IndexRanges : IReadOnlyList<IndexPath>
    {
        private Dictionary<IndexPath, List<IndexRange>>? _ranges;

        public IndexPath this[int index]
        {
            get
            {
                foreach (var r in _ranges!)
                {
                    var parent = r.Key;
                    var ranges = r.Value;
                    var count = IndexRange.GetCount(ranges);

                    if (index < count)
                    {
                        return parent.CloneWithChildIndex(IndexRange.GetAt(ranges, index));
                    }

                    index -= count;
                }

                throw new IndexOutOfRangeException();
            }
        }

        public int Count { get; private set; }

        public void Add(IndexPath index)
        {
            _ranges ??= new Dictionary<IndexPath, List<IndexRange>>();

            var parent = index.GetParent();

            if (!_ranges.TryGetValue(parent, out var ranges))
            {
                ranges = new List<IndexRange>();
                _ranges.Add(parent, ranges);
            }

            IndexRange.Add(ranges, new IndexRange(index.GetLeaf()!.Value));
            ++Count;
        }

        public bool Remove(IndexPath index)
        {
            var parent = index.GetParent();

            if (_ranges is object && _ranges.TryGetValue(parent, out var ranges))
            {
                return IndexRange.Remove(ranges, new IndexRange(index.GetLeaf()!.Value)) > 0;
            }

            return false;
        }

        public bool Contains(IndexPath index)
        {
            var parent = index.GetParent();

            if (_ranges is object && _ranges.TryGetValue(parent, out var ranges))
            {
                return IndexRange.Contains(ranges, index.GetLeaf()!.Value);
            }

            return false;
        }

        public IEnumerator<IndexPath> GetEnumerator()
        {
            if (_ranges is object)
            {
                foreach (var r in _ranges)
                {
                    var parent = r.Key;
                    var ranges = r.Value;
                    var count = IndexRange.GetCount(ranges);

                    for (var i = 0; i < count; ++i)
                        yield return parent.CloneWithChildIndex(IndexRange.GetAt(ranges, i));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
