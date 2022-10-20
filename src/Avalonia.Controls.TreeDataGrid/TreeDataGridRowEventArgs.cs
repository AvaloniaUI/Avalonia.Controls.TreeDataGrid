using System;

namespace Avalonia.Controls
{
    public class TreeDataGridRowEventArgs
    {
        public TreeDataGridRowEventArgs(IControl row, int rowIndex)
        {
            Row = row;
            RowIndex = rowIndex;
        }

        internal TreeDataGridRowEventArgs()
        {
            Row = null!;
        }

        public IControl Row { get; private set; }
        public int RowIndex { get; private set; }

        internal void Update(IControl? row, int rowIndex)
        {
            if (row is object && Row is object)
                throw new NotSupportedException("Nested TreeDataGrid row prepared/clearing detected.");

            Row = row!;
            RowIndex = rowIndex;
        }
    }
}
