using System.Collections.Generic;
using System.Linq;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public abstract class ExpanderCellBase<TModel> : NotifyingBase, IExpanderCell
    {
        private readonly TModel _model;
        private bool _isExpanded;
        private bool _showExpander;

        protected ExpanderCellBase(
            ExpanderColumnBase<TModel> column,
            ExpanderRowBase<TModel> row,
            TModel model,
            IndexPath modelIndex,
            bool showExpander)
        {
            _model = model;
            Column = column;
            Row = row;
            ModelIndex = modelIndex;
            ShowExpander = showExpander;
        }

        public ExpanderColumnBase<TModel> Column { get; }
        public ExpanderRowBase<TModel> Row { get; }
        public IndexPath ModelIndex { get; }

        public bool ShowExpander 
        {
            get => _showExpander;
            private set => RaiseAndSetIfChanged(ref _showExpander, value);
        }

        object? ICell.Value => GetUntypedValue();

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value == _isExpanded)
                    return;

                if (value)
                {
                    if (Column.GetChildModels(_model) is IEnumerable<TModel> childRows && childRows.Any())
                    {
                        Row.Expand(childRows);
                        _isExpanded = true;
                        ShowExpander = true;
                    }
                    else
                    {
                        ShowExpander = false;
                    }
                }
                else
                {
                    Row.Collapse();
                    _isExpanded = false;
                }
            }
        }

        protected abstract object? GetUntypedValue();
    }
}
