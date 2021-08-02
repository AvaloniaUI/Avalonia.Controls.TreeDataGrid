using System;

namespace Avalonia.Controls
{
    public class TreeDataGridCellEventArgs
    {
        public TreeDataGridCellEventArgs(IControl cell, int columnIndex, int rowIndex)
        {
            Cell = cell;
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
        }

        internal TreeDataGridCellEventArgs()
        {
            Cell = null!;
        }

        public IControl Cell { get; private set; }
        public int ColumnIndex { get; private set; }
        public int RowIndex { get; private set; }

        internal void Update(IControl? cell, int columnIndex, int rowIndex)
        {
            if (cell is object && Cell is object)
                throw new NotSupportedException("Nested TreeDataGrid cell prepared/clearing detected.");

            Cell = cell!;
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
        }
    }
}
