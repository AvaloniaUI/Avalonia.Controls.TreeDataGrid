namespace Avalonia.Controls.Models.TreeDataGrid
{
    public abstract class ExpanderCellBase<TModel> : NotifyingBase, IExpanderCell
    {
        private bool _showExpander;

        protected ExpanderCellBase(
            IExpanderColumn<TModel> column,
            IExpanderRow<TModel> row,
            bool showExpander)
        {
            Column = column;
            Row = row;
            ShowExpander = showExpander;
        }

        public IExpanderColumn<TModel> Column { get; }
        public IExpanderRow<TModel> Row { get; }
        IRow IExpanderCell.Row => Row;

        public bool ShowExpander 
        {
            get => _showExpander;
            private set => RaiseAndSetIfChanged(ref _showExpander, value);
        }

        object? ICell.Value => GetUntypedValue();

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

        protected abstract object? GetUntypedValue();
    }
}
