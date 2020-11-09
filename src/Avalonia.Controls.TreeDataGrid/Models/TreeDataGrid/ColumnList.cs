using System.Collections.Generic;
using Avalonia.Collections;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ColumnList<TModel> : AvaloniaList<ColumnBase<TModel>>, IColumns
    {
        IColumn IReadOnlyList<IColumn>.this[int index] => this[index];
        IEnumerator<IColumn> IEnumerable<IColumn>.GetEnumerator() => GetEnumerator();
    }
}
