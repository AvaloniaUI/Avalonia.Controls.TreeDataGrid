using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Layout;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridRowsPresenter : TreeDataGridPresenterBase<IRow>
    {
        public static readonly DirectProperty<TreeDataGridRowsPresenter, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRowsPresenter, IColumns?>(
                nameof(Columns),
                o => o.Columns,
                (o, v) => o.Columns = v);

        public static readonly DirectProperty<TreeDataGridRowsPresenter, ISelectionModel?> SelectionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRowsPresenter, ISelectionModel?>(
                nameof(Selection),
                o => o.Selection,
                (o, v) => o.Selection = v);

        private IColumns? _columns;
        private ISelectionModel? _selection;

        public IColumns? Columns
        {
            get => _columns;
            set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        public ISelectionModel? Selection
        {
            get => _selection;
            set
            {
                if (_selection != value)
                {
                    var oldValue = _selection;

                    if (_selection is object)
                    {
                        _selection.SelectionChanged -= OnSelectionChanged;
                        _selection.IndexesChanged -= OnSelectionIndexesChanged;
                    }

                    _selection = value;

                    if (_selection is object)
                    {
                        _selection.SelectionChanged += OnSelectionChanged;
                        _selection.IndexesChanged += OnSelectionIndexesChanged;
                    }

                    RaisePropertyChanged(
                        SelectionProperty,
                        new Optional<ISelectionModel?>(oldValue),
                        new BindingValue<ISelectionModel?>(_selection));
                }
            }
        }

        protected override Orientation Orientation => Orientation.Vertical;

        protected override (int index, double position) GetElementAt(double position)
        {
            return ((IRows)Items!).GetRowAt(position);
        }

        protected override void RealizeElement(IControl element, IRow rowModel, int index)
        {
            var row = (TreeDataGridRow)element;
            row.Realize(ElementFactory, Columns, (IRows?)Items, index);
            row.IsSelected = _selection?.IsSelected(index) == true;
        }

        protected override void UpdateElementIndex(IControl element, int index)
        {
            ((TreeDataGridRow)element).UpdateIndex(index);
        }

        protected override void UnrealizeElement(IControl element)
        {
            ((TreeDataGridRow)element).Unrealize();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);
            Columns?.MeasureFinished();
            return result;
        }

        private void UpdateSelection()
        {
            foreach (var element in LogicalChildren)
            {
                if (element is TreeDataGridRow row)
                {
                    row.IsSelected = _selection?.IsSelected(row.RowIndex) == true;
                }
            }
        }

        protected override void OnEffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e)
        {
            base.OnEffectiveViewportChanged(sender, e);
            Columns?.ViewportChanged(Viewport);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property == ColumnsProperty)
            {
                var oldValue = change.OldValue.GetValueOrDefault<IColumns>();
                var newValue = change.NewValue.GetValueOrDefault<IColumns>();

                if (oldValue is object)
                    oldValue.LayoutInvalidated -= OnColumnLayoutInvalidated;
                if (newValue is object)
                    newValue.LayoutInvalidated += OnColumnLayoutInvalidated;
            }

            base.OnPropertyChanged(change);
        }

        private void OnColumnLayoutInvalidated(object sender, EventArgs e)
        {
            InvalidateMeasure();
            
            foreach (var element in RealizedElements)
            {
                if (element is TreeDataGridRow row)
                    row.CellsPresenter?.InvalidateMeasure();
            }
        }

        private void OnSelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e)
        {
            UpdateSelection();
        }

        private void OnSelectionIndexesChanged(object sender, SelectionModelIndexesChangedEventArgs e)
        {
            UpdateSelection();
        }
    }
}
