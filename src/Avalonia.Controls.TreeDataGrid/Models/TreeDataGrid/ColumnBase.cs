using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns with a model type.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ColumnBase<TModel> : IColumn, INotifyPropertyChanged
    {
        private ListSortDirection? _sortDirection;

        /// <summary>
        /// Gets or sets the width of the column.
        /// </summary>
        public abstract GridLength Width { get; set; }

        /// <summary>
        /// Gets the column header.
        /// </summary>
        public abstract object? Header { get; }

        /// <summary>
        /// Gets or sets the sort direction indicator that will be displayed on the column.
        /// </summary>
        /// <remarks>
        /// Note that changing this property does not change the sorting of the data, it is only 
        /// used to display a sort direction indicator. To sort data according to a column use
        /// <see cref="ITreeDataGridSource.SortBy(IColumn, ListSortDirection)"/>.
        /// </remarks>
        public ListSortDirection? SortDirection 
        {
            get => _sortDirection;
            set
            {
                if (_sortDirection != value)
                {
                    _sortDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets a comparer function for the column.
        /// </summary>
        /// <param name="direction">The sort direction.</param>
        /// <returns>
        /// The comparer function or null if sorting cannot be performed on the column.
        /// </returns>
        public virtual Func<TModel, TModel, int>? GetComparer(ListSortDirection direction) => null;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
