using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.VisualTree;
using SharedLayoutState = Avalonia.Controls.Primitives.TreeDataGridLayout.SharedLayoutState;

namespace Avalonia.Controls
{
    public class TreeDataGrid : TemplatedControl
    {
        public static readonly DirectProperty<TreeDataGrid, IReadOnlyList<ICell>?> CellsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IReadOnlyList<ICell>?>(
                nameof(Cells),
                o => o.Cells);

        public static readonly DirectProperty<TreeDataGrid, IReadOnlyList<IColumn>?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IReadOnlyList<IColumn>?>(
                nameof(Columns),
                o => o.Columns);

        public static readonly DirectProperty<TreeDataGrid, IElementFactory> ElementFactoryProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IElementFactory>(
                nameof(ElementFactory),
                o => o.ElementFactory);

        public static readonly DirectProperty<TreeDataGrid, Vector> ScrollOffsetProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, Vector>(
                nameof(ScrollOffset),
                o => o.ScrollOffset,
                (o, v) => o.ScrollOffset = v);

        public static readonly DirectProperty<TreeDataGrid, ITreeSelectionModel?> SelectionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, ITreeSelectionModel?>(
                nameof(Selection),
                o => o.Selection,
                (o, v) => o.Selection = v);

        public static readonly DirectProperty<TreeDataGrid, SharedLayoutState> SharedStateProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, SharedLayoutState>(
                nameof(SharedState),
                o => o.SharedState);

        public static readonly DirectProperty<TreeDataGrid, ITreeDataGridSource?> SourceProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, ITreeDataGridSource?>(
                nameof(Source),
                o => o.Source,
                (o, v) => o.Source = v);

        private ITreeDataGridSource? _source;
        private IReadOnlyList<ICell>? _cells;
        private IReadOnlyList<IColumn>? _columns;
        private Vector _scrollOffset;
        private ITreeSelectionModel? _selection;

        public TreeDataGrid()
        {
            ElementFactory = CreateElementFactory();
            SharedState = new SharedLayoutState();
        }

        public IReadOnlyList<ICell>? Cells
        {
            get => _cells;
            private set => SetAndRaise(CellsProperty, ref _cells, value);
        }

        public IReadOnlyList<IColumn>? Columns
        {
            get => _columns;
            private set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        public IElementFactory ElementFactory { get; } = new TreeDataGridElementFactory();

        public ItemsRepeater? Repeater { get; private set; }

        public Vector ScrollOffset
        {
            get => _scrollOffset;
            set => SetAndRaise(ScrollOffsetProperty, ref _scrollOffset, value);
        }

        public ITreeSelectionModel? Selection
        {
            get => _selection;
            set
            {
                if (SetAndRaise(SelectionProperty, ref _selection, value) &&
                    _selection is object)
                {
                    _selection.SelectionChanged += HandleSelectionChanged;
                }
            }
        }

        public SharedLayoutState SharedState { get; }

        public ITreeDataGridSource? Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    _source = value;
                    Cells = _source?.Cells;
                    Columns = _source?.Columns;
                    SetAndRaise(SourceProperty, ref _source, value);
                }
            }
        }

        protected virtual IElementFactory CreateElementFactory() => new TreeDataGridElementFactory();

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            Repeater = e.NameScope.Find<ItemsRepeater>("PART_Cells");
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (_selection is null)
                return;

            var cell = (e.Source as IVisual)?.FindAncestorOfType<TreeDataGridCell>();

            if (cell is object)
            {
                _selection.SelectedItem = _source!.RowToModelHack(cell.RowIndex);
            }
        }

        internal void ColumnRowIndexChanged(TreeDataGridCell cell)
        {
            if (_source is null)
                return;

            // Massive hack.
            var model = _source.RowToModelHack(cell.RowIndex);
            cell.IsSelected = model is object && _selection?.SelectedItem == model;
        }

        private void HandleSelectionChanged(object sender, TreeSelectionModelSelectionChangedEventArgs e)
        {
            if (_source is null || Repeater is null)
                return;

            var selectedRow = _source.ModelToRowHack(_selection?.SelectedItem);

            foreach (var child in Repeater.Children)
            {
                if (child is TreeDataGridCell cell)
                {
                    cell.IsSelected = cell.RowIndex == selectedRow;
                }
            }
        }
    }
}
