using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public abstract class ExpanderCellBase<TModel> : IExpanderCell
    {
        private readonly TModel _model;
        private bool _isExpanded;

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
        public bool ShowExpander { get; }
        object? ICell.Value => GetUntypedValue();

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    Column.IsExpandedChanged(this);
                }
            }
        }

        public IEnumerable<TModel>? GetChildModels() => Column.GetChildModels(_model);

        protected abstract object? GetUntypedValue();
    }
}
