using System;
using System.Collections.Generic;
using System.Linq;

namespace Avalonia.Controls.Selection
{
    public abstract class TreeSelectionModelSelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the items that were removed from the selection.
        /// </summary>
        public IReadOnlyList<object?> DeselectedItems => GetUntypedDeselectedItems();

        /// <summary>
        /// Gets the items that were added to the selection.
        /// </summary>
        public IReadOnlyList<object?> SelectedItems => GetUntypedSelectedItems();

        protected abstract IReadOnlyList<object?> GetUntypedDeselectedItems();
        protected abstract IReadOnlyList<object?> GetUntypedSelectedItems();
    }

    public class TreeSelectionModelSelectionChangedEventArgs<T> : TreeSelectionModelSelectionChangedEventArgs
    {
        private IReadOnlyList<object?>? _deselectedItems;
        private IReadOnlyList<object?>? _selectedItems;

        public TreeSelectionModelSelectionChangedEventArgs(
            IReadOnlyList<T>? deselectedItems = null,
            IReadOnlyList<T>? selectedItems = null)
        {
            DeselectedItems = deselectedItems ?? Array.Empty<T>();
            SelectedItems = selectedItems ?? Array.Empty<T>();
        }

        /// <summary>
        /// Gets the items that were removed from the selection.
        /// </summary>
        public new IReadOnlyList<T> DeselectedItems { get; }

        /// <summary>
        /// Gets the items that were added to the selection.
        /// </summary>
        public new IReadOnlyList<T> SelectedItems { get; }

        protected override IReadOnlyList<object?> GetUntypedDeselectedItems()
        {
            return _deselectedItems ??= (DeselectedItems as IReadOnlyList<object?>) ??
                DeselectedItems.Cast<object?>().ToList();
        }

        protected override IReadOnlyList<object?> GetUntypedSelectedItems()
        {
            return _selectedItems ??= (SelectedItems as IReadOnlyList<object?>) ??
                SelectedItems.Cast<object?>().ToList();
        }
    }
}
