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

        public bool RemoveRange(in IndexPath start, in IndexPath end)
        {
            if (_ranges is null)
                return false;
            if (start.GetSize() == 0)
                throw new ArgumentException("Invalid start index", nameof(start));
            if (end.GetSize() == 0)
                throw new ArgumentException("Invalid end index", nameof(end));

            var result = false;

            for (var i = 0;  i < _ranges.Count; i++)
            {
                var parent = _ranges.Keys[i];
                var ranges = _ranges.Values[i];
                var rangeBegin = parent.CloneWithChildIndex(ranges.First().Begin);
                var rangeEnd = parent.CloneWithChildIndex(ranges.Last().End);
                var depth = parent.GetSize();

                if (start <= rangeBegin && end >= rangeEnd)
                {
                    Count -= IndexRange.GetCount(ranges);
                    _ranges.RemoveAt(i--);
                }
                else if (start < rangeBegin && end < rangeEnd)
                {
                    Count -= IndexRange.Remove(ranges, new IndexRange(0, end.GetAt(depth)));
                }
                else if (start > rangeBegin && end >= rangeEnd)
                {
                    var removeBegin = start.GetAt(depth) + 1;
                    var removeEnd = ranges.Last().End;
                    Count -= IndexRange.Remove(ranges, new IndexRange(removeBegin, removeEnd));
                }
            }

            return result;
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
