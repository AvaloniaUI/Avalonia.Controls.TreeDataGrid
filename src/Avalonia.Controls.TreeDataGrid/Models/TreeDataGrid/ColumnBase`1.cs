using System;
using System.ComponentModel;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns which select cell values from a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ColumnBase<TModel> : NotifyingBase, IColumn<TModel>, IUpdateColumnLayout
    {
        private double _actualWidth = double.NaN;
        private bool? _canUserResize;
        private GridLength _width;
        private GridLength? _minimumWidth;
        private GridLength? _maximumWidth;
        private double _autoWidth;
        private object? _header;
        private ListSortDirection? _sortDirection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="width">
        /// The column width. If null defaults to <see cref="GridLength.Auto"/>.
        /// </param>
        /// <param name="options">Additional column options.</param>
        public ColumnBase(
            object? header,
            GridLength? width,
            ColumnOptions<TModel>? options)
        {
            _canUserResize = options?.CanUserResizeColumn;
            _minimumWidth = options?.MinimumWidth ?? new GridLength(30, GridUnitType.Pixel);
            _maximumWidth = options?.MaximumWidth ?? new GridLength(0, GridUnitType.Star);
            _header = header;
            SetWidth(width ?? GridLength.Auto);
        }

        /// <summary>
        /// Gets the actual width of the column after measurement.
        /// </summary>
        public double ActualWidth
        {
            get => _actualWidth;
            private set => RaiseAndSetIfChanged(ref _actualWidth, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can resize the column.
        /// </summary>
        public bool? CanUserResize
        {
            get => _canUserResize;
            set => RaiseAndSetIfChanged(ref _canUserResize, value);
        }

        /// <summary>
        /// Gets the width of the column.
        /// </summary>
        /// <remarks>
        /// To set the column width use <see cref="IColumns.SetColumnWidth(int, GridLength)"/>.
        /// </remarks>
        public GridLength Width 
        {
            get => _width;
            private set => RaiseAndSetIfChanged(ref _width, value);
        }

        /// <summary>
        /// Gets or sets the column header.
        /// </summary>
        public object? Header
        {
            get => _header;
            set => RaiseAndSetIfChanged(ref _header, value);
        }

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
            set => RaiseAndSetIfChanged(ref _sortDirection, value);
        }

        /// <summary>
        /// Creates a cell for this column on the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The cell.</returns>
        public abstract ICell CreateCell(IRow<TModel> row);

        public abstract Comparison<TModel?>? GetComparison(ListSortDirection direction);

        double IUpdateColumnLayout.CellMeasured(double width, int rowIndex)
        {
            _autoWidth = Math.Max(_autoWidth, width);
            return double.IsNaN(ActualWidth) ? width : ActualWidth;
        }

        void IUpdateColumnLayout.CommitActualWidth()
        {
            if (Width.IsStar)
                return;

            var width = Width.GridUnitType switch
            {
                GridUnitType.Auto => _autoWidth,
                GridUnitType.Pixel => Width.Value,
                _ => throw new NotSupportedException(),
            };

            ActualWidth = CoerceActualWidth(width);
        }

        void IUpdateColumnLayout.CommitActualWidth(double availableWidth, double totalStars)
        {
            if (!Width.IsStar)
                return;

            var width = (availableWidth / totalStars) * Width.Value;
            ActualWidth = CoerceActualWidth(width);
        }

        void IUpdateColumnLayout.SetWidth(GridLength width) => SetWidth(width);

        private double CoerceActualWidth(double width)
        {
            width = _minimumWidth?.GridUnitType switch
            {
                GridUnitType.Auto => Math.Max(width, _autoWidth),
                GridUnitType.Pixel => Math.Max(width, _minimumWidth.Value.Value),
                GridUnitType.Star => throw new NotImplementedException(),
                _ => width
            };

            return _maximumWidth?.GridUnitType switch
            {
                GridUnitType.Auto => Math.Min(width, _autoWidth),
                GridUnitType.Pixel => Math.Min(width, _maximumWidth.Value.Value),
                GridUnitType.Star => throw new NotImplementedException(),
                _ => width
            };
        }

        private void SetWidth(GridLength width)
        {
            _width = width;

            if (width.IsAbsolute)
                ActualWidth = width.Value;
        }
    }
}
