using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Avalonia.Controls.Selection
{
    public interface ITreeSelectionModel : INotifyPropertyChanged
    {
        IEnumerable? Source { get; set; }
        bool SingleSelect { get; set; }
        IndexPath SelectedIndex { get; set; }
        IReadOnlyList<IndexPath> SelectedIndexes { get; }
        object? SelectedItem { get; }
        IReadOnlyList<object?> SelectedItems { get; }
        IndexPath AnchorIndex { get; set; }
        int Count { get; }

        event EventHandler<TreeSelectionModelSelectionChangedEventArgs>? SelectionChanged;
        event EventHandler<TreeSelectionModelIndexesChangedEventArgs>? IndexesChanged;

        void BeginBatchUpdate();
        void Clear();
        void Deselect(IndexPath index);
        void EndBatchUpdate();
        bool IsSelected(IndexPath index);
        void Select(IndexPath index);
    }
}
