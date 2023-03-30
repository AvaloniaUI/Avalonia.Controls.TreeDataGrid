﻿using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    /// <summary>
    /// Stores the realized element state for a <see cref="TreeDataGridPresenterBase{TItem}"/>.
    /// </summary>
    internal struct RealizedElementList
    {
        private int _firstIndex;
        private List<Control?>? _elements;
        private List<double>? _sizes;
        private double _startU;

        /// <summary>
        /// Gets the number of realized elements.
        /// </summary>
        public int Count => _elements?.Count ?? 0;

        /// <summary>
        /// Gets the model index of the first realized element, or -1 if no elements are realized.
        /// </summary>
        public int FirstModelIndex => _elements?.Count > 0 ? _firstIndex : -1;

        /// <summary>
        /// Gets the model index of the last realized element, or -1 if no elements are realized.
        /// </summary>
        public int LastModelIndex => _elements?.Count > 0 ? _firstIndex + _elements.Count - 1 : -1;

        /// <summary>
        /// Gets the elements.
        /// </summary>
        public IReadOnlyList<Control?> Elements => _elements ??= new List<Control?>();

        /// <summary>
        /// Gets the sizes of the elements on the primary axis.
        /// </summary>
        public IReadOnlyList<double> SizeU => _sizes ??= new List<double>();

        /// <summary>
        /// Gets the position of the first element on the primary axis.
        /// </summary>
        public double StartU => _startU;

        /// <summary>
        /// Adds a newly realized element to the collection.
        /// </summary>
        /// <param name="modelIndex">The model index of the element.</param>
        /// <param name="element">The element.</param>
        /// <param name="u">The position of the elemnt on the primary axis.</param>
        /// <param name="sizeU">The size of the element on the primary axis.</param>
        public void Add(int modelIndex, Control element, double u, double sizeU)
        {
            if (modelIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(modelIndex));

            _elements ??= new List<Control?>();
            _sizes ??= new List<double>();

            if (Count == 0)
            {
                _elements.Add(element);
                _sizes.Add(sizeU);
                _startU = u;
                _firstIndex = modelIndex;
            }
            else if (modelIndex == LastModelIndex + 1)
            {
                _elements.Add(element);
                _sizes.Add(sizeU);
            }
            else if (modelIndex == FirstModelIndex - 1)
            {
                --_firstIndex;
                _elements.Insert(0, element);
                _sizes.Insert(0, sizeU);
                _startU = u;
            }
            else
            {
                throw new NotSupportedException("Can only add items to the beginning or end of realized elements.");
            }
        }

        /// <summary>
        /// Gets the element at the specified model index, if realized.
        /// </summary>
        /// <param name="modelIndex">The index in the source collection of the element to get.</param>
        /// <returns>The element if realized; otherwise null.</returns>
        public Control? GetElement(int modelIndex)
        {
            var index = modelIndex - FirstModelIndex;
            if (index >= 0 && index < _elements?.Count)
                return _elements[index];
            return null;
        }

        /// <summary>
        /// Updates the elements in response to items being inserted into the source collection.
        /// </summary>
        /// <param name="modelIndex">The index in the source collection of the insert.</param>
        /// <param name="count">The number of items inserted.</param>
        /// <param name="updateElementIndex">A method used to update the element indexes.</param>
        public void ItemsInserted(int modelIndex, int count, Action<Control, int> updateElementIndex)
        {
            if (modelIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(modelIndex));
            if (_elements is null || _elements.Count == 0)
                return;

            // Get the index within the realized _elements collection.
            var first = FirstModelIndex;
            var index = modelIndex - first;

            if (index < Count)
            {
                // The insertion point affects the realized elements. Update the index of the
                // elements after the insertion point.
                var elementCount = _elements.Count;
                var start = Math.Max(index, 0);

                for (var i = start; i < elementCount; ++i)
                {
                    if (_elements[i] is Control element)
                        updateElementIndex(element, first + i + count);
                }

                if (index <= 0)
                {
                    // The insertion point was before the first element, update the first index.
                    _firstIndex += count;
                }
                else
                {
                    // The insertion point was within the realized elements, insert an empty space
                    // in _elements and _sizes.
                    _elements!.InsertMany(index, null, count);
                    _sizes!.InsertMany(index, 0.0, count);
                }
            }
        }

        /// <summary>
        /// Updates the elements in response to items being removed from the source collection.
        /// </summary>
        /// <param name="modelIndex">The index in the source collection of the remove.</param>
        /// <param name="count">The number of items removed.</param>
        /// <param name="updateElementIndex">A method used to update the element indexes.</param>
        /// <param name="recycleElement">A method used to recycle elements.</param>
        public void ItemsRemoved(
            int modelIndex,
            int count,
            Action<Control, int> updateElementIndex,
            Action<Control> recycleElement)
        {
            if (modelIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(modelIndex));
            if (_elements is null || _elements.Count == 0)
                return;

            // Get the removal start and end index within the realized _elements collection.
            var first = FirstModelIndex;
            var last = LastModelIndex;
            var startIndex = modelIndex - first;
            var endIndex = (modelIndex + count) - first;

            if (endIndex < 0)
            {
                // The removed range was before the realized elements. Update the first index and
                // the indexes of the realized elements.
                _firstIndex -= count;

                for (var i = 0; i < _elements.Count; ++i)
                {
                    if (_elements[i] is Control element)
                        updateElementIndex(element, _firstIndex + i);
                }
            }
            else if (startIndex < _elements.Count)
            {
                // Recycle and remove the affected elements.
                var start = Math.Max(startIndex, 0);
                var end = Math.Min(endIndex, _elements.Count);

                for (var i = start; i < end; ++i)
                {
                    if (_elements[i] is Control element)
                        recycleElement(element);
                }

                _elements.RemoveRange(start, end - start);
                _sizes!.RemoveRange(start, end - start);

                // If the remove started before and ended within our realized elements, then our new
                // first index will be the index where the remove started.
                if (startIndex <= 0 && end < last)
                    _firstIndex = first = modelIndex;

                // Update the indexes of the elements after the removed range.
                end = _elements.Count;
                for (var i = start; i < end; ++i)
                {
                    if (_elements[i] is Control element)
                        updateElementIndex(element, first + i);
                }
            }
        }

        /// <summary>
        /// Recycles elements before a specific index.
        /// </summary>
        /// <param name="modelIndex">The index in the source collection of new first element.</param>
        /// <param name="recycleElement">A method used to recycle elements.</param>
        public void RecycleElementsBefore(int modelIndex, Action<Control> recycleElement)
        {
            if (modelIndex <= FirstModelIndex || _elements is null || _elements.Count == 0)
                return;

            if (modelIndex > LastModelIndex)
            {
                RecycleAllElements(recycleElement);
            }
            else
            {
                var endIndex = modelIndex - FirstModelIndex;

                for (var i = 0; i < endIndex; ++i)
                {
                    if (_elements[i] is Control e)
                        recycleElement(e);
                }

                _elements.RemoveRange(0, endIndex);
                _sizes!.RemoveRange(0, endIndex);
                _firstIndex = modelIndex;
            }
        }

        /// <summary>
        /// Recycles elements after a specific index.
        /// </summary>
        /// <param name="modelIndex">The index in the source collection of new last element.</param>
        /// <param name="recycleElement">A method used to recycle elements.</param>
        public void RecycleElementsAfter(int modelIndex, Action<Control> recycleElement)
        {
            if (modelIndex >= LastModelIndex || _elements is null || _elements.Count == 0)
                return;

            if (modelIndex < FirstModelIndex)
            {
                RecycleAllElements(recycleElement);
            }
            else
            {
                var startIndex = (modelIndex + 1) - FirstModelIndex;
                var count = _elements.Count;

                for (var i = startIndex; i < count; ++i)
                {
                    if (_elements[i] is Control e)
                        recycleElement(e);
                }

                _elements.RemoveRange(startIndex, _elements.Count - startIndex);
                _sizes!.RemoveRange(startIndex, _sizes.Count - startIndex);
            }
        }

        /// <summary>
        /// Recycles all realized elements.
        /// </summary>
        /// <param name="recycleElement">A method used to recycle elements.</param>
        public void RecycleAllElements(Action<Control> recycleElement)
        {
            if (_elements is null || _elements.Count == 0)
                return;

            foreach (var e in _elements)
            {
                if (e is object)
                    recycleElement(e);
            }

            _startU = _firstIndex = 0;
            _elements?.Clear();
            _sizes?.Clear();
        }

        /// <summary>
        /// Resets the element list and prepares it for reuse.
        /// </summary>
        public void ResetForReuse()
        {
            _startU = _firstIndex = 0;
            _elements?.Clear();
            _sizes?.Clear();
        }
    }
}
