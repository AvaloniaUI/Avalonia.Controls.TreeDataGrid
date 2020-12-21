using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Selection
{
    public class TreeSelectionModel<T> : ITreeSelectionModel
        where T : class
    {
        [AllowNull] private T _selectedItem;
        private EventHandler<TreeSelectionModelSelectionChangedEventArgs>? _untypedSelectionChanged;

        [AllowNull]
        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_selectedItem, value!))
                {
                    var oldValue = _selectedItem;
                    _selectedItem = value;

                    if (SelectionChanged is object || _untypedSelectionChanged is object)
                    {
                        var e = new TreeSelectionModelSelectionChangedEventArgs<T>(
                            deselectedItems: oldValue is object ? new[] { oldValue } : null,
                            selectedItems: _selectedItem is object ? new[] { _selectedItem } : null);
                        SelectionChanged?.Invoke(this, e);
                        _untypedSelectionChanged?.Invoke(this, e);
                    }
                }
            }
        }

        object? ITreeSelectionModel.SelectedItem
        {
            get => SelectedItem;
            set => SelectedItem = value as T;
        }

        public event EventHandler<TreeSelectionModelSelectionChangedEventArgs<T>>? SelectionChanged;

        event EventHandler<TreeSelectionModelSelectionChangedEventArgs>? ITreeSelectionModel.SelectionChanged
        {
            add => _untypedSelectionChanged += value;
            remove => _untypedSelectionChanged += value;
        }
    }
}
