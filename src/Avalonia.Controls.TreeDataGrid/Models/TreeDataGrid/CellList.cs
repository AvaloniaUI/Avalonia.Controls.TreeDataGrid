using System;
using System.Collections.Generic;
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

        public override void Clear()
        {
            foreach (var item in this)
            {
                (item as IDisposable)?.Dispose();
            }

            base.Clear();
        }

        public override bool Remove(ICell item)
        {
            (item as IDisposable)?.Dispose();
            return base.Remove(item);
        }

        public override void RemoveAll(IEnumerable<ICell> items)
        {
            foreach (var item in items)
            {
                (item as IDisposable)?.Dispose();
            }

            base.RemoveAll(items);
        }

        public override void RemoveAt(int index)
        {
            (this[index] as IDisposable)?.Dispose();
            base.RemoveAt(index);
        }

        public override void RemoveRange(int index, int count)
        {
            for (var i = index; i < index + count; ++i)
            {
                (this[i] as IDisposable)?.Dispose();
            }

            base.RemoveRange(index, count);
        }
    }
}
