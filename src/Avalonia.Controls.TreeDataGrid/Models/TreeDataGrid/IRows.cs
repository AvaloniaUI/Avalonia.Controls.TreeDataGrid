using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a collection of rows in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    /// <remarks>
    /// Note that items retrieved from an <see cref="IRows"/> collection may be reused, so the
    /// <see cref="IRow"/> should be treated as valid only until the next item is retrieved from
    /// the collection.
    /// </remarks>
    public interface IRows : IReadOnlyList<IRow>, INotifyCollectionChanged
    {
    }
}
