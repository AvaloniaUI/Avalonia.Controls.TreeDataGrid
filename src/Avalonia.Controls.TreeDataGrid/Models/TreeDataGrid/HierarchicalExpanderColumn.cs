using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A column in an <see cref="HierarchicalTreeDataGridSource{TModel}"/> whose cells show an
    /// expander to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class HierarchicalExpanderColumn<TModel> : NotifyingBase,
        IColumn<TModel>,
        IExpanderColumn<TModel>,
        IUpdateColumnLayout
    {
        private readonly IColumn<TModel> _inner;
        private readonly Func<TModel, IEnumerable<TModel>?> _childSelector;
        private readonly Func<TModel, bool>? _hasChildrenSelector;
        private double _actualWidth = double.NaN;

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchicalExpanderColumn{TModel}"/> class.
        /// </summary>
        /// <param name="inner">The inner column which defines how the column will be displayed.</param>
        /// <param name="childSelector">The model children selector.</param>
        /// <param name="hasChildrenSelector">The has model children selector.</param>
        /// <param name="width">The column width.</param>
        public HierarchicalExpanderColumn(
            IColumn<TModel> inner,
            Func<TModel, IEnumerable<TModel>?> childSelector,
            Func<TModel, bool>? hasChildrenSelector = null)
        {
            _inner = inner;
            _inner.PropertyChanged += OnInnerPropertyChanged;
            _childSelector = childSelector;
            _hasChildrenSelector = hasChildrenSelector;
            _actualWidth = inner.ActualWidth;
        }

        /// <summary>
        /// Gets or sets the actual width of the column after measurement.
        /// </summary>
        public double ActualWidth
        {
            get => _actualWidth;
            private set => RaiseAndSetIfChanged(ref _actualWidth, value);
        }

        public bool? CanUserResize => _inner.CanUserResize;
        public object? Header => _inner.Header;

        public ListSortDirection? SortDirection
        {
            get => _inner.SortDirection;
            set => _inner.SortDirection = value;
        }

        public GridLength Width => _inner.Width;

        public ICell CreateCell(IRow<TModel> row)
        {
            if (row is HierarchicalRow<TModel> r)
            {
                return new ExpanderCell<TModel>(_inner.CreateCell(r), r);
            }

            throw new NotSupportedException();
        }

        public bool HasChildren(TModel model) => _hasChildrenSelector?.Invoke(model) ?? true;
        public IEnumerable<TModel>? GetChildModels(TModel model) => _childSelector(model);
        public Comparison<TModel?>? GetComparison(ListSortDirection direction) => _inner.GetComparison(direction);

        double IUpdateColumnLayout.CellMeasured(double width, int rowIndex)
        {
            return ((IUpdateColumnLayout)_inner).CellMeasured(width, rowIndex);
        }

        void IUpdateColumnLayout.SetWidth(GridLength width) => SetWidth(width);

        private void OnInnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CanUserResize) ||
                e.PropertyName == nameof(Header) ||
                e.PropertyName == nameof(SortDirection) ||
                e.PropertyName == nameof(Width))
                RaisePropertyChanged(e);
        }

        private void SetWidth(GridLength width)
        {
            ((IUpdateColumnLayout)_inner).SetWidth(width);

            if (width.IsAbsolute)
                ActualWidth = width.Value;
        }

        void IUpdateColumnLayout.CommitActualWidth() => ((IUpdateColumnLayout)_inner).CommitActualWidth();

        void IUpdateColumnLayout.CommitActualWidth(double availableWidth, double totalStars)
        {
            ((IUpdateColumnLayout)_inner).CommitActualWidth(availableWidth, totalStars);
            ActualWidth = _inner.ActualWidth;
        }
    }
}
