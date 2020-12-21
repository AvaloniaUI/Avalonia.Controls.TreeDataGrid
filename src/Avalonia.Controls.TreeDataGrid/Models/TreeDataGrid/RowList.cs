using System.Collections.Generic;
using Avalonia.Collections;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class RowList<TModel> : AvaloniaList<RowBase<TModel>>, IRows
    {
        IRow IReadOnlyList<IRow>.this[int index] => this[index];
        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => GetEnumerator();
    }
}
