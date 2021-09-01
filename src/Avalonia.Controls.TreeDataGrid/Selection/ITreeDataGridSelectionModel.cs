using System;
using System.Collections.Generic;

namespace Avalonia.Controls.Selection
{
    public interface ITreeDataGridSelectionModel : ITreeSelectionModel
    {
        ISelectionModel RowSelection { get; }
    }

    public interface ITreeDataGridSelectionModel<T> : ITreeDataGridSelectionModel
        where T : class
    {
        new T SelectedItem { get; }
        new IReadOnlyList<object?> SelectedItems { get; }
        new event EventHandler<TreeSelectionModelSelectionChangedEventArgs<T>>? SelectionChanged;
    }
}
