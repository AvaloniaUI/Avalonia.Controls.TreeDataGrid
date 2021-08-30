using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Avalonia.Controls.Selection
{
    internal class IndexRanges : IReadOnlyList<IndexPath>
    {
        private SortedList<IndexPath, List<IndexRange>>? _ranges;

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

        public void Add(in IndexPath index)
        {
            _ranges ??= new();

            var parent = index.GetParent();

            if (!_ranges.TryGetValue(parent, out var ranges))
            {
                ranges = new List<IndexRange>();
                _ranges.Add(parent, ranges);
            }

            IndexRange.Add(ranges, new IndexRange(index.GetLeaf()!.Value));
            ++Count;
        }

        public void Add(in IndexPath parent, in IndexRange range)
        {
            _ranges ??= new();

            if (!_ranges.TryGetValue(parent, out var ranges))
            {
                ranges = new List<IndexRange>();
                _ranges.Add(parent, ranges);
            }

            Count += IndexRange.Add(ranges, range);
        }

        public void Add(in IndexPath parent, List<IndexRange> ranges)
        {
            _ranges ??= new();

            if (!_ranges.TryGetValue(parent, out var r))
            {
                _ranges.Add(parent, ranges);
                Count += IndexRange.GetCount(ranges);
            }
            else
            {
                Count += IndexRange.Add(ranges, r);
            }

        }

        public bool Remove(in IndexPath index)
        {
            var parent = index.GetParent();

            if (_ranges is object && _ranges.TryGetValue(parent, out var ranges))
            {
                if (IndexRange.Remove(ranges, new IndexRange(index.GetLeaf()!.Value)) > 0)
                {
                    --Count;
                    return true;
                }
            }

            return false;
        }

        public bool Contains(in IndexPath index)
        {
            var parent = index.GetParent();

            if (_ranges is object && _ranges.TryGetValue(parent, out var ranges))
            {
                return IndexRange.Contains(ranges, index.GetLeaf()!.Value);
            }

            return false;
        }

        public bool ContainsDescendents(in IndexPath index)
        {
            if (_ranges is object)
            {
                foreach (var i in _ranges.Keys)
                {
                    if (index == i || index.IsAncestorOf(i))
                        return true;
                }
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
