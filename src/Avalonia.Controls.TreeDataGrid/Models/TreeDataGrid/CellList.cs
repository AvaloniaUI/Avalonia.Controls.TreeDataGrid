using System;
using Avalonia.Collections;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class CellList : AvaloniaList<ICell>, ICells
    {
        public CellList(int columCount) => ColumnCount = columCount;
        public ICell this[int column, int row] => this[row * ColumnCount + column];
        public int ColumnCount { get; }
        public int RowCount => ColumnCount > 0 ? Count / ColumnCount : 0;

        public void InsertRow<TModel>(int index, TModel model, Func<TModel, int, ICell> selector)
        {
            if (index > RowCount)
                throw new IndexOutOfRangeException();

            var cellIndex = index * ColumnCount;

            for (var columnIndex = 0; columnIndex < ColumnCount; ++columnIndex)
                Insert(cellIndex++, selector(model, columnIndex));
        }

        public void RemoveRows(int index, int count) => RemoveRange(index * ColumnCount, count * ColumnCount);
    }
}
