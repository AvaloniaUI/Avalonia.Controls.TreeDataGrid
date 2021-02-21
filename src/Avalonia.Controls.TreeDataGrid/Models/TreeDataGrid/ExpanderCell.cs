using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ExpanderCell<TModel> : NotifyingBase, IExpanderCell, IDisposable
    {
        private readonly ICell _inner;
        private bool _showExpander;

        public ExpanderCell(
            ICell inner,
            IExpanderRow<TModel> row,
            bool showExpander)
        {
            _inner = inner;
            Row = row;
            ShowExpander = showExpander;
        }

        public bool CanEdit => _inner.CanEdit;
        public ICell Content => _inner;
        public IExpanderRow<TModel> Row { get; }
        object IExpanderCell.Content => Content;
        IRow IExpanderCell.Row => Row;

        public bool ShowExpander 
        {
            get => _showExpander;
            private set => RaiseAndSetIfChanged(ref _showExpander, value);
        }

        public object? Value => _inner.Value;

        public bool IsExpanded
        {
            get => Row.IsExpanded;
            set
            {
                Row.IsExpanded = value;

                if (value == true && !Row.IsExpanded)
                {
                    ShowExpander = false;
                }
            }
        }

        public void Dispose() => (_inner as IDisposable)?.Dispose();
    }
}
