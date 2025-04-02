using System;
using System.ComponentModel;
using Avalonia.Data;
using Avalonia.Experimental.Data.Core;
using Avalonia.Reactive;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ExpanderCell<TModel> : NotifyingBase,
        IExpanderCell,
        IDisposable
        where TModel : class
    {
        private readonly ICell _inner;
        private readonly IDisposable _subscription;

        public ExpanderCell(
            ICell inner,
            IExpanderRow<TModel> row,
            IObservable<bool> showExpander,
            TypedBindingExpression<TModel, bool>? isExpanded)
        {
            _inner = inner;
            Row = row;
            row.PropertyChanged += RowPropertyChanged;

            var expanderSubscription = showExpander.Subscribe(new AnonymousObserver<bool>(x => Row.UpdateShowExpander(this, x)));
            if (isExpanded is not null)
            {
                var isExpandedSubscription = isExpanded.Subscribe(new AnonymousObserver<BindingValue<bool>>(x =>
                {
                    if (x.HasValue)
                        IsExpanded = x.Value;
                }));
                _subscription = new CompositeDisposable(expanderSubscription, isExpandedSubscription);
            }
            else
            {
                _subscription = expanderSubscription;
            }
        }

        public bool CanEdit => _inner.CanEdit;
        public ICell Content => _inner;
        public BeginEditGestures EditGestures => _inner.EditGestures;
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
            _subscription?.Dispose();
            (_inner as IDisposable)?.Dispose();
        }

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
