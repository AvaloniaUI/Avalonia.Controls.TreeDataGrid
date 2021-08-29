using System;

namespace Avalonia.Controls.Selection
{
    internal struct IndexPathRange
    {
        public IndexPathRange(IndexPath index)
        {
            Begin = index;
            End = index;
        }

        public IndexPathRange(IndexPath begin, IndexPath end)
        {
            // Accept out of order begin/end pairs, just swap them.
            if (begin > end)
            {
                var temp = begin;
                begin = end;
                end = temp;
            }

            Begin = begin;
            End = end;
        }

        public IndexPath Begin { get; }
        public IndexPath End { get; }

        public bool Contains(IndexPath path) => Begin <= path && End >= path;
        public bool FullyContains(IndexPath begin, IndexPath end) => Begin <= begin && End >= end;
    }
}
