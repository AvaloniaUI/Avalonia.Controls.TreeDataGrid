using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Avalonia.Data;
using Avalonia.Experimental.Data.Core;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ExpanderCell<TModel> : NotifyingBase,
        IExpanderCell,
        IDisposable
        where TModel : class
    {
        private readonly ICell _inner;
        private readonly CompositeDisposable? _subscription;
        private bool _showExpander;

        public ExpanderCell(
            ICell inner,
            IExpanderRow<TModel> row,
            IObservable<bool> showExpander,
            TypedBindingExpression<TModel, bool>? isExpanded)
        {
            _inner = inner;
            Row = row;
            row.PropertyChanged += RowPropertyChanged;
            _subscription = new CompositeDisposable(showExpander.Subscribe(x => ShowExpander = x));

            if (isExpanded is not null)
            {
                _subscription.Add(isExpanded.Subscribe(x =>
                {
                    if (x.HasValue)
                        IsExpanded = x.Value;
                }));
            }
        }

        public bool CanEdit => _inner.CanEdit;
        public ICell Content => _inner;
        public IExpanderRow<TModel> Row { get; }

        public bool ShowExpander
        {
            get => _showExpander;
            private set => RaiseAndSetIfChanged(ref _showExpander, value);
        }

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
