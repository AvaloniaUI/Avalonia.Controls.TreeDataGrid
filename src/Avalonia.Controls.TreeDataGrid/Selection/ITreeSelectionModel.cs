using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Selection
{
    public interface ITreeSelectionModel
    {
        object? SelectedItem { get; set; }

        event EventHandler<TreeSelectionModelSelectionChangedEventArgs>? SelectionChanged;
    }
}