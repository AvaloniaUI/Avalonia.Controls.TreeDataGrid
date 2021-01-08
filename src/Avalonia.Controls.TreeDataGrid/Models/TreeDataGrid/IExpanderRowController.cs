using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a controller which receives notifications about an
    /// <see cref="IExpanderRow{TModel}"/>'s state.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IExpanderRowController<TModel>
    {
        /// <summary>
        /// Method called by a row when its children change.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="e"></param>
        void OnChildCollectionChanged(IExpanderRow<TModel> row, NotifyCollectionChangedEventArgs e);
    }
}
