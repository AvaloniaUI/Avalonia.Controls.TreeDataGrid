﻿using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridRowsPresenter : TreeDataGridPresenterBase<IRow>, IChildIndexProvider
    {
        public static readonly DirectProperty<TreeDataGridRowsPresenter, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRowsPresenter, IColumns?>(
                nameof(Columns),
                o => o.Columns,
                (o, v) => o.Columns = v);

        public static readonly DirectProperty<TreeDataGridRowsPresenter, ITreeDataGridSelectionInteraction?> SelectionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridRowsPresenter, ITreeDataGridSelectionInteraction?>(
                nameof(Selection),
                o => o.Selection,
                (o, v) => o.Selection = v);

        private IColumns? _columns;
        private ITreeDataGridSelectionInteraction? _selection;

        public event EventHandler<ChildIndexChangedEventArgs>? ChildIndexChanged;

        public IColumns? Columns
        {
            get => _columns;
            set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        public ITreeDataGridSelectionInteraction? Selection
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
                    }

                    _selection = value;

                    if (_selection is object)
                    {
                        _selection.SelectionChanged += OnSelectionChanged;
                    }

                    RaisePropertyChanged(
                        SelectionProperty,
                        oldValue,
                        _selection);
                }
            }
        }

        protected override Orientation Orientation => Orientation.Vertical;

        protected override (int index, double position) GetElementAt(double position)
        {
            return ((IRows)Items!).GetRowAt(position);
        }

        protected override void RealizeElement(Control element, IRow rowModel, int index)
        {
            var row = (TreeDataGridRow)element;
            row.Realize(ElementFactory, Columns, (IRows?)Items, index);
            row.IsSelected = _selection?.IsRowSelected(rowModel) == true;
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, index));
        }

        protected override void UpdateElementIndex(Control element, int index)
        {
            ((TreeDataGridRow)element).UpdateIndex(index);
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, index));
        }

        protected override void UnrealizeElement(Control element)
        {
            ((TreeDataGridRow)element).Unrealize();
            ChildIndexChanged?.Invoke(this, new ChildIndexChangedEventArgs(element, ((TreeDataGridRow)element).RowIndex));
        }

        private void UpdateSelection()
        {
            foreach (var element in VisualChildren)
            {
                if (element is TreeDataGridRow { RowIndex: >= 0 } row)
                {
                    row.IsSelected = _selection?.IsRowSelected(row.RowIndex) == true;
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Columns?.CommitActualWidths();
            return base.ArrangeOverride(finalSize);
        }

        protected override void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            base.OnEffectiveViewportChanged(sender, e);
            Columns?.ViewportChanged(Viewport);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ColumnsProperty)
            {
                var oldValue = change.GetOldValue<IColumns>();
                var newValue = change.GetNewValue<IColumns>();

                if (oldValue is object)
                    oldValue.LayoutInvalidated -= OnColumnLayoutInvalidated;
                if (newValue is object)
                    newValue.LayoutInvalidated += OnColumnLayoutInvalidated;
            }

            base.OnPropertyChanged(change);
        }

        private void OnColumnLayoutInvalidated(object? sender, EventArgs e)
        {
            InvalidateMeasure();

            foreach (var element in RealizedElements)
            {
                if (element is TreeDataGridRow row)
                    row.CellsPresenter?.InvalidateMeasure();
            }
        }

        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            UpdateSelection();
        }

        public int GetChildIndex(ILogical child)
        {
            if (child is TreeDataGridRow row)
            {
                return row.RowIndex;
            }
            return -1;

        }

        public bool TryGetTotalCount(out int count)
        {
            if (Items != null)
            {
                count = Items.Count;
                return true;
            }
            count = 0;
            return false;
        }
    }
}
