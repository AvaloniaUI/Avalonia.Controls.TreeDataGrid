using System;
using System.ComponentModel;
using Avalonia.Data;
using Avalonia.Experimental.Data.Core;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ExpanderCell<TModel> : NotifyingBase,
        IExpanderCell,
        IObserver<BindingValue<bool>>,
        IDisposable
        where TModel : class
    {
        private readonly ICell _inner;
        private readonly IDisposable? _isExpandedSubscription;

        public ExpanderCell(
            ICell inner,
            IExpanderRow<TModel> row,
            TypedBindingExpression<TModel, bool>? isExpanded)
        {
            _inner = inner;
            Row = row;
            row.PropertyChanged += RowPropertyChanged;
            _isExpandedSubscription = isExpanded?.Subscribe(this);
        }

        public bool CanEdit => _inner.CanEdit;
        public ICell Content => _inner;
        public IExpanderRow<TModel> Row { get; }
        public bool ShowExpander => Row.ShowExpander;
        public object? Value => _inner.Value;

        public bool IsExpanded
        {
            get => Row.IsExpanded;
            set => Row.IsExpanded = value;
        }

        object IExpanderCell.Content => Content;
        IRow IExpanderCell.Row => Row;

        public void Dispose()
        {
            Row.PropertyChanged -= RowPropertyChanged;
            _isExpandedSubscription?.Dispose();
            (_inner as IDisposable)?.Dispose();
        }

        void IObserver<BindingValue<bool>>.OnNext(BindingValue<bool> value)
        {
            if (value.HasValue)
                IsExpanded = value.Value;
        }

        void IObserver<BindingValue<bool>>.OnCompleted() { }
        void IObserver<BindingValue<bool>>.OnError(Exception error) { }

        private void RowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Row.IsExpanded) ||
                e.PropertyName == nameof(Row.ShowExpander))
            {
                RaisePropertyChanged(e.PropertyName);
            }
        }
    }
}
