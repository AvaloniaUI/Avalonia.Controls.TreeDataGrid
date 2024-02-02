using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface IGruppedCell : ICell
    {
        IColumns Columns { get; }
    }
    public class GruppedCell<T> : IGruppedCell
    {
        private readonly IRow<T> _row;
        private readonly IColumns _columns;
        public GruppedCell(IRow<T> row, IColumns columns)
        {
            _row = row;
            _columns = columns;
        }

        public bool CanEdit => false;

        public object? Value => default;

        public BeginEditGestures EditGestures => BeginEditGestures.None;

        public IColumns Columns => _columns;

        internal IEnumerable<ICell> CreateCells()
        {
            foreach (var column in _columns)
            {
                if (column is ColumnBase<T> cb)
                {
                    yield return cb.CreateCell(_row);
                }
            }
        }
    }
}
