using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using SharedLayoutState = Avalonia.Controls.Primitives.TreeDataGridLayout.SharedLayoutState;

namespace Avalonia.Controls
{
    public class TreeDataGrid : TemplatedControl
    {
        public static readonly StyledProperty<bool> CanUserSortColumnsProperty =
            AvaloniaProperty.Register<TreeDataGridPanel, bool>(nameof(CanUserSortColumns), true);

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

        public static readonly DirectProperty<TreeDataGrid, ISelectionModel?> SelectionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, ISelectionModel?>(
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
        private ISelectionModel? _selection;
        private IControl? _userSortColumn;
        private ListSortDirection _userSortDirection;

        public TreeDataGrid()
        {
            ElementFactory = CreateElementFactory();
            SharedState = new SharedLayoutState();
            AddHandler(TreeDataGridColumnHeader.ClickEvent, OnClick);
        }

        public bool CanUserSortColumns
        {
            get => GetValue(CanUserSortColumnsProperty);
            set => SetValue(CanUserSortColumnsProperty, value);
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

        public IElementFactory ElementFactory { get; } = new TreeListElementFactory();

        public ItemsRepeater? Repeater { get; private set; }

        public Vector ScrollOffset
        {
            get => _scrollOffset;
            set => SetAndRaise(ScrollOffsetProperty, ref _scrollOffset, value);
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
                        _selection.SelectionChanged -= OnSelectionChanged;

                    _selection = value;

                    if (_selection is object)
                        _selection.SelectionChanged += OnSelectionChanged;

                    RaisePropertyChanged(
                        SelectionProperty,
                        new Optional<ISelectionModel?>(oldValue),
                        new BindingValue<ISelectionModel?>(_selection));
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

        protected virtual IElementFactory CreateElementFactory() => new TreeListElementFactory();

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (Repeater is object)
            {
                Repeater.ElementPrepared -= OnElementPrepared;
            }

            Repeater = e.NameScope.Find<ItemsRepeater>("PART_Cells");

            if (Repeater is object)
            {
                Repeater.ElementClearing += OnElementClearing;
                Repeater.ElementPrepared += OnElementPrepared;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (Source is null || _selection is null || e.Handled)
                return;

            var cell = (e.Source as IVisual).FindAncestorOfType<ITreeDataGridCell>();
            var rows = Source.Rows;

            if (cell is null || cell.RowIndex < 0 || cell.RowIndex >= rows.Count)
                return;

            _selection.Select(cell.RowIndex);
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (_source is null)
                return;

            if (e.Source is TreeDataGridColumnHeader columnHeader &&
                columnHeader.Model is object &&
                CanUserSortColumns)
            {
                if (_userSortColumn != columnHeader)
                {
                    _userSortColumn = columnHeader;
                    _userSortDirection = ListSortDirection.Ascending;
                }
                else
                {
                    _userSortDirection = _userSortDirection == ListSortDirection.Ascending ?
                        ListSortDirection.Descending : ListSortDirection.Ascending;
                }

                _source.SortBy(columnHeader.Model, _userSortDirection);
            }
        }

        private void OnElementPrepared(object sender, ItemsRepeaterElementPreparedEventArgs e)
        {
            if (Source is null || !(e.Element is ITreeDataGridCell cell))
                return;

            cell.ColumnIndex = e.Index % Source.Columns.Count;
            cell.RowIndex = e.Index / Source.Columns.Count;
            cell.IsSelected = _selection?.IsSelected(cell.RowIndex) ?? false;
        }

        private void OnElementClearing(object sender, ItemsRepeaterElementClearingEventArgs e)
        {
            if (!(e.Element is ITreeDataGridCell cell))
                return;
            cell.ColumnIndex = cell.RowIndex = -1;
        }

        private void OnSelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e)
        {
            if (_source is null || _selection is null || Repeater is null)
                return;

            foreach (var child in Repeater.Children)
            {
                if (child is ITreeDataGridCell cell)
                {
                    cell.IsSelected = _selection.IsSelected(cell.RowIndex);
                }
            }
        }
    }
}
