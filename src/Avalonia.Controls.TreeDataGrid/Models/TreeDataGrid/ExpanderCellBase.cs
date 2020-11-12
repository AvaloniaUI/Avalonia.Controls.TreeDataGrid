using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public abstract class ExpanderCellBase<TModel> : NotifyingBase, IExpanderCell
    {
        private readonly TModel _model;
        private bool _isExpanded;
        private bool _showExpander;

        protected ExpanderCellBase(
            ExpanderColumnBase<TModel> column,
            TModel model,
            IndexPath modelIndex,
            bool showExpander)
        {
            _model = model;
            Column = column;
            ModelIndex = modelIndex;
            ShowExpander = showExpander;
        }

        public ExpanderColumnBase<TModel> Column { get; }
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
                if (_isExpanded != value)
                {
                    _isExpanded = value;

                    if (_isExpanded)
                    {
                        if (Column.TryExpand(this))
                        {
                            RaisePropertyChanged();
                        }
                        else
                        {
                            _isExpanded = false;
                            ShowExpander = false;
                        }
                    }
                    else
                    {
                        Column.Collapse(this);
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public IEnumerable<TModel>? GetChildModels() => Column.GetChildModels(_model);

        protected abstract object? GetUntypedValue();
    }
}
