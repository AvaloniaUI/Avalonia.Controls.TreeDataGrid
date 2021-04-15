using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    internal interface ITreeDataGridCell : IControl, ISelectable
    {
        int ColumnIndex { get; }
        int RowIndex { get; }

        void Realize(IElementFactory factory, ICell model, int columnIndex, int RowIndex);
        void Unrealize();
    }
}
