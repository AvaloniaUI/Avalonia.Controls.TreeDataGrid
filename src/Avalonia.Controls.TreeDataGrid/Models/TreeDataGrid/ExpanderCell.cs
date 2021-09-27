using System;
using System.ComponentModel;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ExpanderCell<TModel> : NotifyingBase, IExpanderCell, IDisposable
    {
        private readonly ICell _inner;

        public ExpanderCell(
            ICell inner,
            IExpanderRow<TModel> row)
        {
            _inner = inner;
            Row = row;
            row.PropertyChanged += RowPropertyChanged;
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
