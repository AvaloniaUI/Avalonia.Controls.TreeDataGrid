using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    internal interface ITreeDataGridCell
    {
        int ColumnIndex { get; }

        void Realize(TreeDataGridElementFactory factory, ICell model, int columnIndex, int rowIndex);
        void Unrealize();
    }
}
