// This source file is adapted from the dotnet runtime project.
// (https://github.com/dotnet/runtime)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Controls.Selection
{

    internal static partial class Enumerable
    {
        public static IOrderedEnumerable<TSource> OrderByWithSelectionPreserving<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer,
            ISelectionModel selection) =>
            new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false, null, selection);
    }
    /// <summary>
    /// An iterator that can produce an array or <see cref="List{TElement}"/> through an optimized path.
    /// </summary>
    internal interface IIListProvider<TElement> : IEnumerable<TElement>
    {
        /// <summary>
        /// Produce an array of the sequence through an optimized path.
        /// </summary>
        /// <returns>The array.</returns>
        TElement[] ToArray();

        /// <summary>
        /// Produce a <see cref="List{TElement}"/> of the sequence through an optimized path.
        /// </summary>
        /// <returns>The <see cref="List{TElement}"/>.</returns>
        List<TElement> ToList();

        /// <summary>
        /// Returns the count of elements in the sequence.
        /// </summary>
        /// <param name="onlyIfCheap">If true then the count should only be calculated if doing
        /// so is quick (sure or likely to be constant time), otherwise -1 should be returned.</param>
        /// <returns>The number of elements.</returns>
        int GetCount(bool onlyIfCheap);
    }
    /// <summary>
    /// A buffer into which the contents of an <see cref="IEnumerable{TElement}"/> can be stored.
    /// </summary>
    /// <typeparam name="TElement">The type of the buffer's elements.</typeparam>
    internal readonly struct Buffer<TElement>
    {
        /// <summary>
        /// The stored items.
        /// </summary>
        internal readonly TElement[] _items;

        /// <summary>
        /// The number of stored items.
        /// </summary>
        internal readonly int _count;

        /// <summary>
        /// Fully enumerates the provided enumerable and stores its items into an array.
        /// </summary>
        /// <param name="source">The enumerable to be store.</param>
        internal Buffer(IEnumerable<TElement> source)
        {
            if (source is IIListProvider<TElement> iterator)
            {
                TElement[] array = iterator.ToArray();
                _items = array;
                _count = array.Length;
            }
            else
            {
                _items = ToArray(source, out _count);
            }
        }
        internal static T[] ToArray<T>(IEnumerable<T> source, out int length)
        {
            if (source is ICollection<T> ic)
            {
                int count = ic.Count;
                if (count != 0)
                {
                    // Allocate an array of the desired size, then copy the elements into it. Note that this has the same
                    // issue regarding concurrency as other existing collections like List<T>. If the collection size
                    // concurrently changes between the array allocation and the CopyTo, we could end up either getting an
                    // exception from overrunning the array (if the size went up) or we could end up not filling as many
                    // items as 'count' suggests (if the size went down).  This is only an issue for concurrent collections
                    // that implement ICollection<T>, which as of .NET 4.6 is just ConcurrentDictionary<TKey, TValue>.
                    T[] arr = new T[count];
                    ic.CopyTo(arr, 0);
                    length = count;
                    return arr;
                }
            }
            else
            {
                using (var en = source.GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        const int DefaultCapacity = 4;
                        T[] arr = new T[DefaultCapacity];
                        arr[0] = en.Current;
                        int count = 1;

                        while (en.MoveNext())
                        {
                            if (count == arr.Length)
                            {
                                // This is the same growth logic as in List<T>:
                                // If the array is currently empty, we make it a default size.  Otherwise, we attempt to
                                // double the size of the array.  Doubling will overflow once the size of the array reaches
                                // 2^30, since doubling to 2^31 is 1 larger than Int32.MaxValue.  In that case, we instead
                                // constrain the length to be Array.MaxLength (this overflow check works because of the
                                // cast to uint).
                                int newLength = count << 1;
                                if ((uint)newLength > 0X7FFFFFC7)
                                {
                                    newLength = 0X7FFFFFC7 <= count ? count + 1 : 0X7FFFFFC7;
                                }

                                Array.Resize(ref arr, newLength);
                            }

                            arr[count++] = en.Current;
                        }

                        length = count;
                        return arr;
                    }
                }
            }

            length = 0;
            return Array.Empty<T>();
        }
    }
    internal abstract partial class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement>
    {
        internal IEnumerable<TElement> _source;
        private readonly ISelectionModel selection;

        protected OrderedEnumerable(IEnumerable<TElement> source, ISelectionModel selection)
        {
            _source = source;
            this.selection = selection;
        }

        private int[] SortedMap(Buffer<TElement> buffer) => GetEnumerableSorter().Sort(buffer._items, buffer._count);

        private int[] SortedMap(Buffer<TElement> buffer, int minIdx, int maxIdx) =>
            GetEnumerableSorter().Sort(buffer._items, buffer._count, minIdx, maxIdx);

        public IEnumerator<TElement> GetEnumerator()
        {
            Buffer<TElement> buffer = new Buffer<TElement>(_source);
            if (buffer._count > 0)
            {
                int[] map = SortedMap(buffer);
                if (selection.SelectedIndexes.Count > 0)
                {
                    //take the copy of SelectedIndexes collection because it could be modified at the runtime
                    var indexes = selection.SelectedIndexes.ToList();
                    for (int i = 0; i < selection.SelectedIndexes.Count; i++)
                    {
                        for (int j = 0; j < map.Length; j++)
                        {
                            if (map[j] == indexes[i])
                            {
                                selection.Deselect(indexes[i]);
                                selection.Select(j);
                            }
                        }
                    }
                }
                for (int i = 0; i < buffer._count; i++)
                {
                    yield return buffer._items[map[i]];
                }
            }
        }

        internal IEnumerator<TElement> GetEnumerator(int minIdx, int maxIdx)
        {
            Buffer<TElement> buffer = new Buffer<TElement>(_source);
            int count = buffer._count;
            if (count > minIdx)
            {
                if (count <= maxIdx)
                {
                    maxIdx = count - 1;
                }

                if (minIdx == maxIdx)
                {
                    yield return GetEnumerableSorter().ElementAt(buffer._items, count, minIdx);
                }
                else
                {
                    int[] map = SortedMap(buffer, minIdx, maxIdx);
                    while (minIdx <= maxIdx)
                    {
                        yield return buffer._items[map[minIdx]];
                        ++minIdx;
                    }
                }
            }
        }

        private EnumerableSorter<TElement> GetEnumerableSorter() => GetEnumerableSorter(null);

        internal abstract EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement>? next);

        private CachingComparer<TElement> GetComparer() => GetComparer(null);

        internal abstract CachingComparer<TElement> GetComparer(CachingComparer<TElement>? childComparer);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey>? comparer, bool descending) =>
            new OrderedEnumerable<TElement, TKey>(_source, keySelector, comparer, @descending, this, selection);

        public TElement? TryGetLast(Func<TElement, bool> predicate, out bool found)
        {
            CachingComparer<TElement> comparer = GetComparer();
            using (IEnumerator<TElement> e = _source.GetEnumerator())
            {
                TElement value;
                do
                {
                    if (!e.MoveNext())
                    {
                        found = false;
                        return default;
                    }

                    value = e.Current;
                }
                while (!predicate(value));

                comparer.SetElement(value);
                while (e.MoveNext())
                {
                    TElement x = e.Current;
                    if (predicate(x) && comparer.Compare(x, false) >= 0)
                    {
                        value = x;
                    }
                }

                found = true;
                return value;
            }
        }
    }

    internal sealed class OrderedEnumerable<TElement, TKey> : OrderedEnumerable<TElement>
    {
        private readonly OrderedEnumerable<TElement>? _parent;
        private readonly Func<TElement, TKey> _keySelector;
        private readonly IComparer<TKey> _comparer;
        private readonly bool _descending;

        internal OrderedEnumerable(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey>? comparer, bool descending, OrderedEnumerable<TElement>? parent, ISelectionModel selection) :
            base(source, selection)
        {
            if (source is null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector is null)
            {
                throw new ArgumentNullException("keySelector");
            }

            _parent = parent;
            _keySelector = keySelector;
            _comparer = comparer ?? Comparer<TKey>.Default;
            _descending = descending;
        }

        internal override EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement>? next)
        {
            // Special case the common use of string with default comparer. Comparer<string>.Default checks the
            // thread's Culture on each call which is an overhead which is not required, because we are about to
            // do a sort which remains on the current thread (and EnumerableSorter is not used afterwards).
            IComparer<TKey> comparer = _comparer;
            if (typeof(TKey) == typeof(string) && comparer == Comparer<string>.Default)
            {
                comparer = (IComparer<TKey>)StringComparer.CurrentCulture;
            }

            EnumerableSorter<TElement> sorter = new EnumerableSorter<TElement, TKey>(_keySelector, comparer, _descending, next);
            if (_parent != null)
            {
                sorter = _parent.GetEnumerableSorter(sorter);
            }

            return sorter;
        }

        internal override CachingComparer<TElement> GetComparer(CachingComparer<TElement>? childComparer)
        {
            CachingComparer<TElement> cmp = childComparer == null
                ? new CachingComparer<TElement, TKey>(_keySelector, _comparer, _descending)
                : new CachingComparerWithChild<TElement, TKey>(_keySelector, _comparer, _descending, childComparer);
            return _parent != null ? _parent.GetComparer(cmp) : cmp;
        }
    }

    // A comparer that chains comparisons, and pushes through the last element found to be
    // lower or higher (depending on use), so as to represent the sort of comparisons
    // done by OrderBy().ThenBy() combinations.
    internal abstract class CachingComparer<TElement>
    {
        internal abstract int Compare(TElement element, bool cacheLower);

        internal abstract void SetElement(TElement element);
    }

    internal class CachingComparer<TElement, TKey> : CachingComparer<TElement>
    {
        protected readonly Func<TElement, TKey> _keySelector;
        protected readonly IComparer<TKey> _comparer;
        protected readonly bool _descending;
        protected TKey? _lastKey;

        public CachingComparer(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            _keySelector = keySelector;
            _comparer = comparer;
            _descending = descending;
        }

        internal override int Compare(TElement element, bool cacheLower)
        {
            TKey newKey = _keySelector(element);
            int cmp = _descending ? _comparer.Compare(_lastKey, newKey) : _comparer.Compare(newKey, _lastKey);
            if (cacheLower == cmp < 0)
            {
                _lastKey = newKey;
            }

            return cmp;
        }

        internal override void SetElement(TElement element)
        {
            _lastKey = _keySelector(element);
        }
    }

    internal sealed class CachingComparerWithChild<TElement, TKey> : CachingComparer<TElement, TKey>
    {
        private readonly CachingComparer<TElement> _child;

        public CachingComparerWithChild(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, CachingComparer<TElement> child)
            : base(keySelector, comparer, descending)
        {
            _child = child;
        }

        internal override int Compare(TElement element, bool cacheLower)
        {
            TKey newKey = _keySelector(element);
            int cmp = _descending ? _comparer.Compare(_lastKey, newKey) : _comparer.Compare(newKey, _lastKey);
            if (cmp == 0)
            {
                return _child.Compare(element, cacheLower);
            }

            if (cacheLower == cmp < 0)
            {
                _lastKey = newKey;
                _child.SetElement(element);
            }

            return cmp;
        }

        internal override void SetElement(TElement element)
        {
            base.SetElement(element);
            _child.SetElement(element);
        }
    }

    internal abstract class EnumerableSorter<TElement>
    {
        internal abstract void ComputeKeys(TElement[] elements, int count);

        internal abstract int CompareAnyKeys(int index1, int index2);

        private int[] ComputeMap(TElement[] elements, int count)
        {
            ComputeKeys(elements, count);
            int[] map = new int[count];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = i;
            }

            return map;
        }

        internal int[] Sort(TElement[] elements, int count)
        {
            int[] map = ComputeMap(elements, count);
            QuickSort(map, 0, count - 1);
            return map;
        }

        internal int[] Sort(TElement[] elements, int count, int minIdx, int maxIdx)
        {
            int[] map = ComputeMap(elements, count);
            PartialQuickSort(map, 0, count - 1, minIdx, maxIdx);
            return map;
        }

        internal TElement ElementAt(TElement[] elements, int count, int idx)
        {
            int[] map = ComputeMap(elements, count);
            return idx == 0 ?
                elements[Min(map, count)] :
                elements[QuickSelect(map, count - 1, idx)];
        }

        protected abstract void QuickSort(int[] map, int left, int right);

        // Sorts the k elements between minIdx and maxIdx without sorting all elements
        // Time complexity: O(n + k log k) best and average case. O(n^2) worse case.
        protected abstract void PartialQuickSort(int[] map, int left, int right, int minIdx, int maxIdx);

        // Finds the element that would be at idx if the collection was sorted.
        // Time complexity: O(n) best and average case. O(n^2) worse case.
        protected abstract int QuickSelect(int[] map, int right, int idx);

        protected abstract int Min(int[] map, int count);
    }

    internal sealed partial class ArraySortHelper<T>
    {
        #region IArraySortHelper<T> Members

        public void Sort(Span<T> keys, IComparer<T>? comparer)
        {
            // Add a try block here to detect IComparers (or their
            // underlying IComparables, etc) that are bogus.
            try
            {
                comparer ??= Comparer<T>.Default;
                IntrospectiveSort(keys, comparer.Compare);
            }
            catch (IndexOutOfRangeException)
            {
                // ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            }
            catch (Exception e)
            {
                // ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        public int BinarySearch(T[] array, int index, int length, T value, IComparer<T>? comparer)
        {
            try
            {
                comparer ??= Comparer<T>.Default;
                return InternalBinarySearch(array, index, length, value, comparer);
            }
            catch (Exception e)
            {
                //   ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
                return 0;
            }
        }

        #endregion

        internal static void Sort(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(comparer != null, "Check the arguments in the caller!");

            // Add a try block here to detect bogus comparisons
            try
            {
                IntrospectiveSort(keys, comparer);
            }
            catch (IndexOutOfRangeException)
            {
                //  ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            }
            catch (Exception e)
            {
                //  ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        internal static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
        {
            Debug.Assert(array != null, "Check the arguments in the caller!");
            Debug.Assert(index >= 0 && length >= 0 && (array.Length - index >= length), "Check the arguments in the caller!");

            int lo = index;
            int hi = index + length - 1;
            while (lo <= hi)
            {
                int i = lo + ((hi - lo) >> 1);
                int order = comparer.Compare(array[i], value);

                if (order == 0) return i;
                if (order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        private static void SwapIfGreater(Span<T> keys, Comparison<T> comparer, int i, int j)
        {
            Debug.Assert(i != j);

            if (comparer(keys[i], keys[j]) > 0)
            {
                T key = keys[i];
                keys[i] = keys[j];
                keys[j] = key;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(Span<T> a, int i, int j)
        {
            Debug.Assert(i != j);

            T t = a[i];
            a[i] = a[j];
            a[j] = t;
        }

        internal static void IntrospectiveSort(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(comparer != null);

            if (keys.Length > 1)
            {
                IntroSort(keys, 2 * (BitOperations.Log2((uint)keys.Length) + 1), comparer);
            }
        }

        private static void IntroSort(Span<T> keys, int depthLimit, Comparison<T> comparer)
        {
            Debug.Assert(!keys.IsEmpty);
            Debug.Assert(depthLimit >= 0);
            Debug.Assert(comparer != null);

            int partitionSize = keys.Length;
            while (partitionSize > 1)
            {
                if (partitionSize <= 16)
                {

                    if (partitionSize == 2)
                    {
                        SwapIfGreater(keys, comparer, 0, 1);
                        return;
                    }

                    if (partitionSize == 3)
                    {
                        SwapIfGreater(keys, comparer, 0, 1);
                        SwapIfGreater(keys, comparer, 0, 2);
                        SwapIfGreater(keys, comparer, 1, 2);
                        return;
                    }

                    InsertionSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }
                depthLimit--;

                int p = PickPivotAndPartition(keys.Slice(0, partitionSize), comparer);

                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys[(p + 1)..partitionSize], depthLimit, comparer);
                partitionSize = p;
            }
        }

        private static int PickPivotAndPartition(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(keys.Length >= 16);
            Debug.Assert(comparer != null);

            int hi = keys.Length - 1;

            // Compute median-of-three.  But also partition them, since we've done the comparison.
            int middle = hi >> 1;

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreater(keys, comparer, 0, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, 0, hi);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, hi); // swap the middle with the high

            T pivot = keys[middle];
            Swap(keys, middle, hi - 1);
            int left = 0, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right)
            {
                while (comparer(keys[++left], pivot) < 0) ;
                while (comparer(pivot, keys[--right]) < 0) ;

                if (left >= right)
                    break;

                Swap(keys, left, right);
            }

            // Put pivot in the right location.
            if (left != hi - 1)
            {
                Swap(keys, left, hi - 1);
            }
            return left;
        }

        private static void HeapSort(Span<T> keys, Comparison<T> comparer)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(!keys.IsEmpty);

            int n = keys.Length;
            for (int i = n >> 1; i >= 1; i--)
            {
                DownHeap(keys, i, n, comparer);
            }

            for (int i = n; i > 1; i--)
            {
                Swap(keys, 0, i - 1);
                DownHeap(keys, 1, i - 1, comparer);
            }
        }

        private static void DownHeap(Span<T> keys, int i, int n, Comparison<T> comparer)
        {
            Debug.Assert(comparer != null);

            T d = keys[i - 1];
            while (i <= n >> 1)
            {
                int child = 2 * i;
                if (child < n && comparer(keys[child - 1], keys[child]) < 0)
                {
                    child++;
                }

                if (!(comparer(d, keys[child - 1]) < 0))
                    break;

                keys[i - 1] = keys[child - 1];
                i = child;
            }

            keys[i - 1] = d;
        }

        private static void InsertionSort(Span<T> keys, Comparison<T> comparer)
        {
            for (int i = 0; i < keys.Length - 1; i++)
            {
                T t = keys[i + 1];

                int j = i;
                while (j >= 0 && comparer(t, keys[j]) < 0)
                {
                    keys[j + 1] = keys[j];
                    j--;
                }

                keys[j + 1] = t;
            }
        }
    }
    internal sealed class EnumerableSorter<TElement, TKey> : EnumerableSorter<TElement>
    {
        private readonly Func<TElement, TKey> _keySelector;
        private readonly IComparer<TKey> _comparer;
        private readonly bool _descending;
        private readonly EnumerableSorter<TElement>? _next;
        private TKey[]? _keys;

        internal EnumerableSorter(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, EnumerableSorter<TElement>? next)
        {
            _keySelector = keySelector;
            _comparer = comparer;
            _descending = descending;
            _next = next;
        }

        internal override void ComputeKeys(TElement[] elements, int count)
        {
            _keys = new TKey[count];
            for (int i = 0; i < count; i++)
            {
                _keys[i] = _keySelector(elements[i]);
            }

            _next?.ComputeKeys(elements, count);
        }

        internal override int CompareAnyKeys(int index1, int index2)
        {
            Debug.Assert(_keys != null);

            int c = _comparer.Compare(_keys[index1], _keys[index2]);
            if (c == 0)
            {
                if (_next == null)
                {
                    return index1 - index2; // ensure stability of sort
                }

                return _next.CompareAnyKeys(index1, index2);
            }

            // -c will result in a negative value for int.MinValue (-int.MinValue == int.MinValue).
            // Flipping keys earlier is more likely to trigger something strange in a comparer,
            // particularly as it comes to the sort being stable.
            return (_descending != (c > 0)) ? 1 : -1;
        }

        private int CompareKeys(int index1, int index2) => index1 == index2 ? 0 : CompareAnyKeys(index1, index2);

        public static void Sort<T>(Span<T> span, Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            if (span.Length > 1)
            {
                ArraySortHelper<T>.Sort(span, comparison);
            }
        }
        protected override void QuickSort(int[] keys, int lo, int hi) =>
           Sort(new Span<int>(keys, lo, hi - lo + 1), CompareAnyKeys);

        // Sorts the k elements between minIdx and maxIdx without sorting all elements
        // Time complexity: O(n + k log k) best and average case. O(n^2) worse case.
        protected override void PartialQuickSort(int[] map, int left, int right, int minIdx, int maxIdx)
        {
            do
            {
                int i = left;
                int j = right;
                int x = map[i + ((j - i) >> 1)];
                do
                {
                    while (i < map.Length && CompareKeys(x, map[i]) > 0)
                    {
                        i++;
                    }

                    while (j >= 0 && CompareKeys(x, map[j]) < 0)
                    {
                        j--;
                    }

                    if (i > j)
                    {
                        break;
                    }

                    if (i < j)
                    {
                        int temp = map[i];
                        map[i] = map[j];
                        map[j] = temp;
                    }

                    i++;
                    j--;
                }
                while (i <= j);

                if (minIdx >= i)
                {
                    left = i + 1;
                }
                else if (maxIdx <= j)
                {
                    right = j - 1;
                }

                if (j - left <= right - i)
                {
                    if (left < j)
                    {
                        PartialQuickSort(map, left, j, minIdx, maxIdx);
                    }

                    left = i;
                }
                else
                {
                    if (i < right)
                    {
                        PartialQuickSort(map, i, right, minIdx, maxIdx);
                    }

                    right = j;
                }
            }
            while (left < right);
        }

        // Finds the element that would be at idx if the collection was sorted.
        // Time complexity: O(n) best and average case. O(n^2) worse case.
        protected override int QuickSelect(int[] map, int right, int idx)
        {
            int left = 0;
            do
            {
                int i = left;
                int j = right;
                int x = map[i + ((j - i) >> 1)];
                do
                {
                    while (i < map.Length && CompareKeys(x, map[i]) > 0)
                    {
                        i++;
                    }

                    while (j >= 0 && CompareKeys(x, map[j]) < 0)
                    {
                        j--;
                    }

                    if (i > j)
                    {
                        break;
                    }

                    if (i < j)
                    {
                        int temp = map[i];
                        map[i] = map[j];
                        map[j] = temp;
                    }

                    i++;
                    j--;
                }
                while (i <= j);

                if (i <= idx)
                {
                    left = i + 1;
                }
                else
                {
                    right = j - 1;
                }

                if (j - left <= right - i)
                {
                    if (left < j)
                    {
                        right = j;
                    }

                    left = i;
                }
                else
                {
                    if (i < right)
                    {
                        left = i;
                    }

                    right = j;
                }
            }
            while (left < right);

            return map[idx];
        }

        protected override int Min(int[] map, int count)
        {
            int index = 0;
            for (int i = 1; i < count; i++)
            {
                if (CompareKeys(map[i], map[index]) < 0)
                {
                    index = i;
                }
            }
            return map[index];
        }
    }
}


