using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Utilities;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class GrouppedColumnn<TModel> : ColumnList<TModel>, IGruppedColumn, IColumn<TModel>, IUpdateColumnLayout
        where TModel : class
    {
        private static readonly ColumnOptions<TModel> _defaultColumnOptions = new();
        private double _actualWidth = double.NaN;
        private GridLength _width;
        private double _autoWidth = double.NaN;
        private double _starWidth = double.NaN;
        private bool _starWidthWasConstrained;
        private object? _header;
        private ListSortDirection? _sortDirection;
        private readonly Comparison<TModel?> _comparison;

        public GrouppedColumnn(
            object? header,
            GridLength? width = default,
            ColumnOptions<TModel>? options = default)
        {
            _header = header;
            Options = options ?? new();
            _comparison = GrouppedColumnnComparison;
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
        /// Gets the column options.
        /// </summary>
        public ColumnOptions<TModel> Options { get; }

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
        /// Gets or sets a user-defined object attached to the column.
        /// </summary>
        public object? Tag { get; set; }

        bool? IColumn.CanUserResize => Options.CanUserResizeColumn;
        double IUpdateColumnLayout.MinActualWidth => CoerceActualWidth(0);
        double IUpdateColumnLayout.MaxActualWidth => CoerceActualWidth(double.PositiveInfinity);
        bool IUpdateColumnLayout.StarWidthWasConstrained => _starWidthWasConstrained;

        /// <summary>
        /// Creates a cell for this column on the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The cell.</returns>
        public ICell CreateCell(IRow<TModel> row) =>
            new GruppedCell<TModel>(row, this);

        public Comparison<TModel?>? GetComparison(ListSortDirection direction) =>
            _comparison;

        double IUpdateColumnLayout.CellMeasured(double width, int rowIndex)
        {
            double autoWidth = 0.0;
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] is IUpdateColumnLayout columnLayout)
                {
                    var w = columnLayout.CellMeasured(width, rowIndex);
                    autoWidth += w;
                }
            }
            _autoWidth = Math.Max(NonNaN(_autoWidth), CoerceActualWidth(width));
            return Width.GridUnitType == GridUnitType.Auto || double.IsNaN(ActualWidth) ?
                _autoWidth : ActualWidth;
        }

        void IUpdateColumnLayout.CalculateStarWidth(double availableWidth, double totalStars)
        {
            if (!Width.IsStar)
                throw new InvalidOperationException("Attempt to calculate star width on a non-star column.");

            var width = (availableWidth / totalStars) * Width.Value;
            _starWidth = CoerceActualWidth(width);
            _starWidthWasConstrained = !MathUtilities.AreClose(_starWidth, width);
        }

        bool IUpdateColumnLayout.CommitActualWidth()
        {
            var width = Width.GridUnitType switch
            {
                GridUnitType.Auto => _autoWidth,
                GridUnitType.Pixel => CoerceActualWidth(Width.Value),
                GridUnitType.Star => _starWidth,
                _ => throw new NotSupportedException(),
            };

            var oldWidth = ActualWidth;
            ActualWidth = width;
            _starWidthWasConstrained = false;
            return !MathUtilities.AreClose(oldWidth, ActualWidth);
        }

        void IUpdateColumnLayout.SetWidth(GridLength width) => SetWidth(width);

        private double CoerceActualWidth(double width)
        {
            width = Options.MinWidth.GridUnitType switch
            {
                GridUnitType.Auto => Math.Max(width, _autoWidth),
                GridUnitType.Pixel => Math.Max(width, Options.MinWidth.Value),
                GridUnitType.Star => throw new NotImplementedException(),
                _ => width
            };

            return Options.MaxWidth?.GridUnitType switch
            {
                GridUnitType.Auto => Math.Min(width, _autoWidth),
                GridUnitType.Pixel => Math.Min(width, Options.MaxWidth.Value.Value),
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

        private static double NonNaN(double v) => double.IsNaN(v) ? 0 : v;

        private int GrouppedColumnnComparison(TModel? x, TModel? y)
        {
            var result = 0;
            var direction = SortDirection ?? ListSortDirection.Ascending;
            foreach (var item in this.OfType<ColumnBase<TModel>>())
            {
                if (item.GetComparison(direction) is { } comparer)
                {
                    result = comparer(x, y);
                    if (result != 0)
                    {
                        break;
                    }
                }
            }
            return result;
        }
    }
}
