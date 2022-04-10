using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;

namespace Avalonia.Controls.Selection
{
    /// <summary>
    /// Defines the interaction between a <see cref="TreeDataGrid"/> and an
    /// <see cref="ITreeDataGridSelection"/> model.
    /// </summary>
    public interface ITreeDataGridSelectionInteraction
    {
        public event EventHandler? SelectionChanged;

        bool IsRowSelected(IRow rowModel);
        bool IsRowSelected(int rowIndex);
        public void OnKeyDown(TreeDataGrid sender, KeyEventArgs e) { }
        public void OnKeyUp(TreeDataGrid sender, KeyEventArgs e) { }
        public void OnTextInput(TreeDataGrid sender, TextInputEventArgs e) { }
        public void OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e) { }
        public void OnPointerMoved(TreeDataGrid sender, PointerEventArgs e) { }
        public void OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e) { }
    }
}
