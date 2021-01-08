using System;
using Avalonia.Collections;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// An implementation of <see cref="ICells"/> that stores its cells in a list.
    /// </summary>
    public class CellList : AvaloniaList<ICell>, ICells
    {
        public CellList(int columnCount) => ColumnCount = columnCount;
        
        public ICell this[int column, int row]
        {
            get => this[row * ColumnCount + column];
            set => this[row * ColumnCount + column] = value;
        }
        
        public int ColumnCount { get; }
        public int RowCount => ColumnCount > 0 ? Count / ColumnCount : 0;
    }
}
