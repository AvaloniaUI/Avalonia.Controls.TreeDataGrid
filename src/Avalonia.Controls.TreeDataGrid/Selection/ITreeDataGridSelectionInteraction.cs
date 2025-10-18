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

        bool IsCellSelected(int columnIndex, int rowIndex);
        bool IsRowSelected(IRow rowModel);
        bool IsRowSelected(int rowIndex);
        void OnKeyDown(TreeDataGrid sender, KeyEventArgs e);
        void OnPreviewKeyDown(TreeDataGrid sender, KeyEventArgs e);
        void OnKeyUp(TreeDataGrid sender, KeyEventArgs e);
        void OnTextInput(TreeDataGrid sender, TextInputEventArgs e);
        void OnPointerPressed(TreeDataGrid sender, PointerPressedEventArgs e);
        void OnPointerMoved(TreeDataGrid sender, PointerEventArgs e);
        void OnPointerReleased(TreeDataGrid sender, PointerReleasedEventArgs e);
    }
}
