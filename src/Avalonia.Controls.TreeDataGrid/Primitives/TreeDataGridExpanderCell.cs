using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridExpanderCell : TreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridExpanderCell, int> IndentProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, int>(
                nameof(Indent),
                o => o.Indent);

        public static readonly DirectProperty<TreeDataGridExpanderCell, bool> IsExpandedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, bool>(
                nameof(IsExpanded),
                o => o.IsExpanded,
                (o, v) => o.IsExpanded = v);

        public static readonly DirectProperty<TreeDataGridExpanderCell, bool> ShowExpanderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, bool>(
                nameof(ShowExpander),
                o => o.ShowExpander);

        private Decorator? _contentContainer;
        private Type? _contentType;
        private IElementFactory? _factory;
        private ElementFactoryGetArgs? _getArgs;
        private int _indent;
        private bool _isExpanded;
        private IExpanderCell? _model;
        private ElementFactoryRecycleArgs? _recycleArgs;
        private bool _showExpander;

        public int Indent
        {
            get => _indent;
            private set => SetAndRaise(IndentProperty, ref _indent, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetAndRaise(IsExpandedProperty, ref _isExpanded, value) && _model is object)
                    _model.IsExpanded = value;
            }
        }

        public bool ShowExpander
        {
            get => _showExpander;
            private set => SetAndRaise(ShowExpanderProperty, ref _showExpander, value);
        }

        public override void Realize(IElementFactory factory, ICell model, int columnIndex, int rowIndex)
        {
            if (_model is object)
                throw new InvalidOperationException("Cell is already realized.");

            if (model is IExpanderCell expanderModel)
            {
                _factory = factory;
                _model = expanderModel;
                Indent = (_model.Row as IIndentedRow)?.Indent ?? 0;
                IsExpanded = _model.IsExpanded;
                ShowExpander = _model.ShowExpander;
            }
            else
            {
                throw new InvalidOperationException("Invalid cell model.");
            }

            base.Realize(factory, model, columnIndex, rowIndex);
            UpdateContent(_factory);
        }

        public override void Unrealize()
        {
            _model = null;
            base.Unrealize();
            if (_factory is object)
                UpdateContent(_factory);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _contentContainer = e.NameScope.Find<Decorator>("PART_Content");
            if (_factory is object)
                UpdateContent(_factory);
        }

        private void UpdateContent(IElementFactory factory)
        {
            if (_contentContainer is null || _model is null)
                return;

            if (_model?.Content is ICell innerModel)
            {
                var contentType = innerModel.GetType();

                if (contentType != _contentType)
                {
                    _getArgs ??= new ElementFactoryGetArgs();
                    _getArgs.Data = innerModel;
                    _getArgs.Index = ColumnIndex;

                    var element = factory.GetElement(_getArgs);

                    element.IsVisible = true;
                    _contentContainer.Child = element;
                    _contentType = contentType;
                }

                if (_contentContainer.Child is ITreeDataGridCell innerCell)
                    innerCell.Realize(factory, innerModel, ColumnIndex, RowIndex);
            }
            else
            {
                var element = _contentContainer.Child;
                _contentContainer.Child = null;
                _recycleArgs ??= new ElementFactoryRecycleArgs();
                _recycleArgs.Element = element;
                factory.RecycleElement(_recycleArgs);
            }
        }
    }
}
