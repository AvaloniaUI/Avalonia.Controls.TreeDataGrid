// This source file is adapted from the WinUI project.
// (https://github.com/microsoft/microsoft-ui-xaml)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#nullable enable

namespace Avalonia.Controls
{
    /// <summary>
    /// Temporary local copy of ItemsSourceView until the upstream one is fixed to only subscribe to
    /// _inner.CollectionChanged when it has CollectionChanged listeners.
    /// </remarks>
    public class ItemsSourceViewFix : INotifyCollectionChanged
    {
        /// <summary>
        ///  Gets an empty <see cref="ItemsSourceViewFix"/>
        /// </summary>
        public static ItemsSourceViewFix Empty { get; } = new ItemsSourceViewFix(Array.Empty<object>());

        private protected readonly IList _inner;
        private NotifyCollectionChangedEventHandler? _collectionChanged;

        /// <summary>
        /// Initializes a new instance of the ItemsSourceView class for the specified data source.
        /// </summary>
        /// <param name="source">The data source.</param>
        public ItemsSourceViewFix(IEnumerable source)
        {
            source = source ?? throw new ArgumentNullException(nameof(source));

            if (source is IList list)
            {
                _inner = list;
            }
            else if (source is IEnumerable<object> objectEnumerable)
            {
                _inner = new List<object>(objectEnumerable);
            }
            else
            {
                _inner = new List<object>(source.Cast<object>());
            }
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count => _inner.Count;

        /// <summary>
        /// Gets a value that indicates whether the items source can provide a unique key for each item.
        /// </summary>
        /// <remarks>
        /// TODO: Not yet implemented in Avalonia.
        /// </remarks>
        public bool HasKeyIndexMapping => false;

        /// <summary>
        /// Retrieves the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
        public object? this[int index] => GetAt(index);

        /// <summary>
        /// Occurs when the collection has changed to indicate the reason for the change and which items changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                if (_collectionChanged is null)
                {
                    if (_inner is INotifyCollectionChanged incc)
                    {
                        incc.CollectionChanged += OnCollectionChanged;
                    }
                }

                _collectionChanged += value;
            }

            remove
            {
                _collectionChanged -= value;

                if (_collectionChanged is null)
                {
                    if (_inner is INotifyCollectionChanged incc)
                    {
                        incc.CollectionChanged -= OnCollectionChanged;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // TODO: Remove IDisposable from ItemsSourceView
        }

        /// <summary>
        /// Retrieves the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
        public object? GetAt(int index) => _inner[index];

        public int IndexOf(object? item) => _inner.IndexOf(item);

        public static ItemsSourceViewFix GetOrCreate(IEnumerable? items)
        {
            if (items is ItemsSourceViewFix isv)
            {
                return isv;
            }
            else if (items is null)
            {
                return Empty;
            }
            else
            {
                return new ItemsSourceViewFix(items);
            }
        }

        /// <summary>
        /// Retrieves the index of the item that has the specified unique identifier (key).
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The key</returns>
        /// <remarks>
        /// TODO: Not yet implemented in Avalonia.
        /// </remarks>
        public string KeyFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the unique identifier (key) for the item at the specified index.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The index.</returns>
        /// <remarks>
        /// TODO: Not yet implemented in Avalonia.
        /// </remarks>
        public int IndexFromKey(string key)
        {
            throw new NotImplementedException();
        }

        protected void OnItemsSourceChanged(NotifyCollectionChangedEventArgs args)
        {
            _collectionChanged?.Invoke(this, args);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnItemsSourceChanged(e);
        }
    }

    /// Temporary local copy of ItemsSourceView until the upstream one is fixed to only subscribe to
    /// _inner.CollectionChanged when it has CollectionChanged listeners.
    public class ItemsSourceViewFix<T> : ItemsSourceViewFix, IReadOnlyList<T>
    {
        /// <summary>
        ///  Gets an empty <see cref="ItemsSourceViewFix"/>
        /// </summary>
        public new static ItemsSourceViewFix<T> Empty { get; } = new ItemsSourceViewFix<T>(Array.Empty<T>());

        /// <summary>
        /// Initializes a new instance of the ItemsSourceView class for the specified data source.
        /// </summary>
        /// <param name="source">The data source.</param>
        public ItemsSourceViewFix(IEnumerable<T> source)
            : base(source)
        {
        }

        private ItemsSourceViewFix(IEnumerable source)
            : base(source)
        {
        }

        /// <summary>
        /// Retrieves the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
#pragma warning disable CS8603
        public new T this[int index] => GetAt(index);
#pragma warning restore CS8603

        /// <summary>
        /// Retrieves the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
        [return: MaybeNull]
        public new T GetAt(int index) => (T)_inner[index];

        public IEnumerator<T> GetEnumerator() => _inner.Cast<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

        public static new ItemsSourceViewFix<T> GetOrCreate(IEnumerable? items)
        {
            if (items is ItemsSourceViewFix<T> isv)
            {
                return isv;
            }
            else if (items is null)
            {
                return Empty;
            }
            else
            {
                return new ItemsSourceViewFix<T>(items);
            }
        }
    }
}
