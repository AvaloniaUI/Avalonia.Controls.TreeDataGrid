using System;
using System.Collections.Specialized;
using Avalonia.Collections;

namespace Avalonia.Controls.TreeDataGridTests
{
    internal class AvaloniaListDebug<T> : AvaloniaList<T>, INotifyCollectionChanged
    {
        private NotifyCollectionChangedEventHandler? _collectionChanged;
        
        event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
        {
            add
            {
                base.CollectionChanged += value;
                _collectionChanged += value;
            }
            remove
            {
                base.CollectionChanged += value;
                _collectionChanged -= value;   
            }
        }

        public Delegate[]? GetCollectionChangedSubscribers() => _collectionChanged?.GetInvocationList();
    }
}
