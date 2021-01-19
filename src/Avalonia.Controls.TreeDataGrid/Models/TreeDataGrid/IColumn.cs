using System.ComponentModel;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a column in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface IColumn : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets a value indicating whether the user can resize the column.
        /// </summary>
        bool? CanUserResize { get; }

        /// <summary>
        /// Gets the column header.
        /// </summary>
        object? Header { get; }

        /// <summary>
        /// Gets the width of the column.
        /// </summary>
        GridLength Width { get; set; }

        /// <summary>
        /// Gets or sets the sort direction indicator that will be displayed on the column.
        /// </summary>
        /// <remarks>
        /// Note that changing this property does not change the sorting of the data, it is only 
        /// used to display a sort direction indicator. To sort data according to a column use
        /// <see cref="ITreeDataGridSource.SortBy(IColumn, ListSortDirection)"/>.
        /// </remarks>
        ListSortDirection? SortDirection { get; set; }
    }
}
