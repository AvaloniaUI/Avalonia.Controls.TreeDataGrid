using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using Avalonia.Experimental.Data;
using Avalonia.Experimental.Data.Core;
using Avalonia.Reactive;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns which select cell values from a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TModel">The value type.</typeparam>
    public abstract class ColumnBase<TModel, TValue> : ColumnBase<TModel>
        where TModel : class
    {
        private bool _canUserSort;
        private readonly Comparison<TModel>? _sortAscending;
        private readonly Comparison<TModel>? _sortDescending;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="width">The column width.</param>
        public ColumnBase(
            object? header,
            Expression<Func<TModel, TValue>> valueSelector,
            GridLength? width,
            ColumnOptions<TModel>? options)
            : base(header, width, options)
        {
            ValueSelector = valueSelector.Compile();
            Binding = TypedBinding<TModel>.OneWay(valueSelector);
            _canUserSort = options?.CanUserSortColumn ?? true;
            _sortAscending = options?.CompareAscending ?? DefaultSortAscending;
            _sortDescending = options?.CompareDescending ?? DefaultSortDescending;
        }

        /// <summary>
        /// Gets the function which selects the column value from the model.
        /// </summary>
        public Func<TModel, TValue> ValueSelector { get; }

        /// <summary>
        /// Gets a binding which selects the column value from the model.
        /// </summary>
        public TypedBinding<TModel, TValue> Binding { get; }

        public override Comparison<TModel>? GetComparison(ListSortDirection direction)
        {
            if (!_canUserSort)
                return null;
            
            return direction switch
            {
                ListSortDirection.Ascending => _sortAscending,
                ListSortDirection.Descending => _sortDescending,
                _ => null,
            };
        }

        protected TypedBindingExpression<TModel, TValue> CreateBindingExpression(TModel model)
        {
            return new TypedBindingExpression<TModel, TValue>(
                ObservableEx.SingleValue(model),
                Binding.Read!,
                Binding.Write,
                Binding.Links!,
                default);
        }

        private int DefaultSortAscending(TModel x, TModel y)
        {
            var a = ValueSelector(x);
            var b = ValueSelector(y);
            return Comparer<TValue>.Default.Compare(a, b);
        }

        private int DefaultSortDescending(TModel x, TModel y)
        {
            var a = ValueSelector(x);
            var b = ValueSelector(y);
            return Comparer<TValue>.Default.Compare(b, a);
        }
    }
}
