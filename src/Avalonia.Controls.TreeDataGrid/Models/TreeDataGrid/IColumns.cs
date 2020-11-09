using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a collection of columns in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface IColumns : IReadOnlyList<IColumn>, INotifyCollectionChanged
    {
    }
}
