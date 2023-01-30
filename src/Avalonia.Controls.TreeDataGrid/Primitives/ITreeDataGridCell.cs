using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    internal interface ITreeDataGridCell :  ISelectable
    {
        int ColumnIndex { get; }

        void Realize(IElementFactory factory, ICell model, int columnIndex, int rowIndex);
        void Unrealize();
    }
}
