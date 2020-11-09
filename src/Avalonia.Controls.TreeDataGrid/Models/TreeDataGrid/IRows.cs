using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a collection of rows in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface IRows : IReadOnlyList<IRow>, INotifyCollectionChanged
    {
    }
}
