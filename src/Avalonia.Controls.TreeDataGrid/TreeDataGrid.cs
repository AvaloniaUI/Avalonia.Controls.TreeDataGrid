using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        public static readonly StyledProperty<bool> CanUserResizeColumnsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserResizeColumns), true);

        public static readonly StyledProperty<bool> CanUserSortColumnsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserSortColumns), true);

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

        public bool CanUserResizeColumns
        {
            get => GetValue(CanUserResizeColumnsProperty);
            set => SetValue(CanUserResizeColumnsProperty, value);
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

        public bool TryGetRowModel<TModel>(IControl element, [MaybeNullWhen(false)] out TModel result)
        {
            if (Source is object &&
                TryGetCell(element, out var cell) &&
                cell.RowIndex < Source.Rows.Count &&
                Source.Rows[cell.RowIndex] is IRow<TModel> row)
            {
                result = row.Model;
                return true;
            }

            result = default;
            return false;
        }

        protected virtual IElementFactory CreateElementFactory() => new TreeListElementFactory();

        protected bool MoveSelection(NavigationDirection direction, bool rangeModifier)
        {
            return MoveSelection(direction, rangeModifier, GetFocusedCell());
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (Repeater is object)
            {
                Repeater.ElementClearing -= OnElementClearing;
                Repeater.ElementPrepared -= OnElementPrepared;
                Repeater.ElementIndexChanged -= OnElementIndexChanged;
            }

            Repeater = e.NameScope.Find<ItemsRepeater>("PART_Cells");

            if (Repeater is object)
            {
                Repeater.ElementClearing += OnElementClearing;
                Repeater.ElementPrepared += OnElementPrepared;
                Repeater.ElementIndexChanged += OnElementIndexChanged;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                var direction = e.Key.ToNavigationDirection();
                var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                if (direction.HasValue)
                {
                    var focused = GetFocusedCell();

                    if (focused is object && !ctrl)
                    {
                        e.Handled = TryKeyExpandCollapse(direction.Value, focused);
                    }

                    if (!e.Handled && (!ctrl || shift))
                    {
                        e.Handled = MoveSelection(direction.Value, shift, focused);
                    }
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (Source is null || _selection is null || e.Handled)
                return;

            if (e.Source is IControl source && TryGetCell(source, out var cell))
            {
                var point = e.GetCurrentPoint(this);

                UpdateSelection(
                    cell.RowIndex,
                    select: true,
                    rangeModifier: e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                    toggleModifier: e.KeyModifiers.HasFlag(KeyModifiers.Control),
                    rightButton: point.Properties.IsRightButtonPressed);
                e.Handled = true;
            }
        }

        private ITreeDataGridCell? GetFocusedCell()
        {
            var focus = FocusManager.Instance;
            ITreeDataGridCell? focused = null;
            if (focus.Current is IControl current)
                TryGetCell(current, out focused);
            return focused;
        }

        private bool MoveSelection(NavigationDirection direction, bool rangeModifier, ITreeDataGridCell? focused)
        {
            if (Source is null || Repeater is null)
                return false;

            var currentColumnIndex = focused?.ColumnIndex ?? 0;
            var currentRowIndex = focused?.RowIndex ?? Selection?.SelectedIndex ?? 0;
            var newColumnIndex = currentColumnIndex;
            int newRowIndex;

            if (direction == NavigationDirection.First || direction == NavigationDirection.Last)
            {
                newRowIndex = direction == NavigationDirection.First ? 0 : Source.Rows.Count - 1;
            }
            else
            {
                (int x, int y) step = direction switch
                {
                    NavigationDirection.Up => (0, -1),
                    NavigationDirection.Down => (0, 1),
                    NavigationDirection.Left => (-1, 0),
                    NavigationDirection.Right => (1, 0),
                    _ => (0, 0)
                };

                newColumnIndex = Math.Max(0, Math.Min(currentColumnIndex + step.x, Source.Columns.Count - 1));
                newRowIndex = Math.Max(0, Math.Min(currentRowIndex + step.y, Source.Rows.Count - 1));
            }

            if (newRowIndex != currentRowIndex)
                UpdateSelection(newRowIndex, true, rangeModifier);

            if (newRowIndex != currentRowIndex || newColumnIndex != currentColumnIndex)
            {
                var cellIndex = (newRowIndex * Source.Columns.Count) + newColumnIndex;
                var cell = Repeater.GetOrCreateElement(cellIndex);

                if (cell is object)
                {
                    (VisualRoot as TopLevel)?.LayoutManager?.ExecuteLayoutPass();
                    cell.BringIntoView();

                    var method = direction <= NavigationDirection.Previous ?
                        NavigationMethod.Tab :
                        NavigationMethod.Directional;
                    FocusManager.Instance?.Focus(cell, method);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryKeyExpandCollapse(NavigationDirection direction, ITreeDataGridCell focused)
        {
            if (Source is null || Repeater is null || focused.RowIndex < 0)
                return false;

            var row = Source.Rows[focused.RowIndex];

            if (row is IExpander expander)
            {
                if (direction == NavigationDirection.Right && !expander.IsExpanded)
                {
                    expander.IsExpanded = true;
                    return true;
                }
                else if (direction == NavigationDirection.Left && expander.IsExpanded)
                {
                    expander.IsExpanded = false;
                    return true;
                }
            }

            return false;
        }

        private bool TryGetCell(IControl? element, [MaybeNullWhen(false)] out ITreeDataGridCell result)
        {
            if (element is ITreeDataGridCell cell && cell.RowIndex >= 0)
            {
                result = cell;
                return true;
            }

            do
            {
                result = (element as IVisual)?.FindAncestorOfType<ITreeDataGridCell>();
                if (result?.ColumnIndex >= 0 && result?.RowIndex >= 0)
                    break;
                element = result;
            } while (result is object);

            return result is object;
        }

        private void UpdateSelection(
            int index,
            bool select = true,
            bool rangeModifier = false,
            bool toggleModifier = false,
            bool rightButton = false)
        {
            if (Source is null || _selection is null || index < 0 || index >= Source.Rows.Count)
            {
                return;
            }

            var mode = _selection.SingleSelect ? SelectionMode.Single : SelectionMode.Multiple;
            var multi = (mode & SelectionMode.Multiple) != 0;
            var toggle = (toggleModifier || (mode & SelectionMode.Toggle) != 0);
            var range = multi && rangeModifier;

            if (!select)
            {
                _selection.Deselect(index);
            }
            else if (rightButton)
            {
                if (_selection.IsSelected(index) == false)
                {
                    _selection.SelectedIndex = index;
                }
            }
            else if (range)
            {
                using var operation = _selection.BatchUpdate();
                _selection.Clear();
                _selection.SelectRange(_selection.AnchorIndex, index);
            }
            else if (multi && toggle)
            {
                if (_selection.IsSelected(index) == true)
                {
                    _selection.Deselect(index);
                }
                else
                {
                    _selection.Select(index);
                }
            }
            else if (toggle)
            {
                _selection.SelectedIndex = (_selection.SelectedIndex == index) ? -1 : index;
            }
            else
            {
                _selection.SelectedIndex = index;
            }
        }

        private void UpdateSelection()
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

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (_source is object &&
                e.Source is TreeDataGridColumnHeader columnHeader &&
                columnHeader.DataContext is IColumn column &&
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

                if (_source.SortBy(column, _userSortDirection))
                    _selection?.Clear();
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

        private void OnElementIndexChanged(object sender, ItemsRepeaterElementIndexChangedEventArgs e)
        {
            if (Source is null || !(e.Element is ITreeDataGridCell cell))
                return;

            cell.ColumnIndex = e.NewIndex % Source.Columns.Count;
            cell.RowIndex = e.NewIndex / Source.Columns.Count;
            cell.IsSelected = _selection?.IsSelected(cell.RowIndex) ?? false;
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
