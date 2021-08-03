using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls
{
    public class TreeDataGrid : TemplatedControl
    {
        public static readonly StyledProperty<bool> CanUserResizeColumnsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserResizeColumns), true);

        public static readonly StyledProperty<bool> CanUserSortColumnsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserSortColumns), true);

        public static readonly DirectProperty<TreeDataGrid, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IColumns?>(
                nameof(Columns),
                o => o.Columns);

        public static readonly DirectProperty<TreeDataGrid, IElementFactory> ElementFactoryProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IElementFactory>(
                nameof(ElementFactory),
                o => o.ElementFactory);

        public static readonly DirectProperty<TreeDataGrid, IRows?> RowsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IRows?>(
                nameof(Rows),
                o => o.Rows,
                (o, v) => o.Rows = v);

        public static readonly DirectProperty<TreeDataGrid, IScrollable?> ScrollProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IScrollable?>(
                nameof(Scroll),
                o => o.Scroll);

        public static readonly StyledProperty<bool> ShowColumnHeadersProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(ShowColumnHeaders), true);

        public static readonly DirectProperty<TreeDataGrid, ISelectionModel?> SelectionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, ISelectionModel?>(
                nameof(Selection),
                o => o.Selection,
                (o, v) => o.Selection = v);

        public static readonly DirectProperty<TreeDataGrid, ITreeDataGridSource?> SourceProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, ITreeDataGridSource?>(
                nameof(Source),
                o => o.Source,
                (o, v) => o.Source = v);

        private ITreeDataGridSource? _source;
        private IColumns? _columns;
        private IRows? _rows;
        private IScrollable? _scroll;
        private ISelectionModel? _selection;
        private IControl? _userSortColumn;
        private ListSortDirection _userSortDirection;
        private TreeDataGridCellEventArgs? _cellArgs;

        public TreeDataGrid()
        {
            ElementFactory = CreateElementFactory();
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

        public IColumns? Columns
        {
            get => _columns;
            private set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        public IElementFactory ElementFactory { get; } = new TreeDataGridElementFactory();

        public IRows? Rows
        {
            get => _rows;
            private set => SetAndRaise(RowsProperty, ref _rows, value);
        }

        public TreeDataGridColumnHeadersPresenter? ColumnHeadersPresenter { get; private set; }
        public TreeDataGridRowsPresenter? RowsPresenter { get; private set; }
        
        public IScrollable? Scroll 
        {
            get => _scroll;
            private set => SetAndRaise(ScrollProperty, ref _scroll, value);
        }

        public bool ShowColumnHeaders
        {
            get => GetValue(ShowColumnHeadersProperty);
            set => SetValue(ShowColumnHeadersProperty, value);
        }

        public ISelectionModel? Selection
        {
            get => _selection;
            set => SetAndRaise(SelectionProperty, ref _selection, value);
        }

        public ITreeDataGridSource? Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    if (value != null)
                    {
                        value.Sorted += Source_Sorted;
                    }
                    if (_source!=null)
                    {
                        _source.Sorted -= Source_Sorted;
                    }
                    void Source_Sorted()
                    {
                        RowsPresenter?.RecycleAllElements();
                        RowsPresenter?.InvalidateMeasure();
                    }

                    var oldSource = _source;
                    _source = value;
                    Columns = _source?.Columns;
                    Rows = _source?.Rows;
                    RaisePropertyChanged(
                        SourceProperty,
                        new Optional<ITreeDataGridSource?>(oldSource),
                        new BindingValue<ITreeDataGridSource?>(oldSource));
                }
            }
        }

        public event EventHandler<TreeDataGridCellEventArgs>? CellClearing;
        public event EventHandler<TreeDataGridCellEventArgs>? CellPrepared;
        public event CancelEventHandler SelectionChanging;

        public IControl? TryGetCell(int columnIndex, int rowIndex)
        {
            if (TryGetRow(rowIndex) is TreeDataGridRow row &&
                row.TryGetCell(columnIndex) is IControl cell)
            {
                return cell;
            }

            return null;
        }

        public TreeDataGridRow? TryGetRow(int rowIndex)
        {
            return RowsPresenter?.TryGetElement(rowIndex) as TreeDataGridRow;
        }

        public bool TryGetRowModel<TModel>(IControl element, [MaybeNullWhen(false)] out TModel result)
        {
            if (Source is object &&
                TryGetRow(element, out var row) &&
                row.RowIndex < Source.Rows.Count &&
                Source.Rows[row.RowIndex] is IRow<TModel> rowWithModel)
            {
                result = rowWithModel.Model;
                return true;
            }

            result = default;
            return false;
        }

        protected virtual IElementFactory CreateElementFactory() => new TreeDataGridElementFactory();

        protected bool MoveSelection(NavigationDirection direction, bool rangeModifier)
        {
            return MoveSelection(direction, rangeModifier, GetFocusedRow());
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            ColumnHeadersPresenter = e.NameScope.Find<TreeDataGridColumnHeadersPresenter>("PART_ColumnHeadersPresenter");
            RowsPresenter = e.NameScope.Find<TreeDataGridRowsPresenter>("PART_RowsPresenter");
            Scroll = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
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
                    var focused = GetFocusedRow();

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

            if (e.Source is IControl source && TryGetRow(source, out var row))
            {
                var point = e.GetCurrentPoint(this);

                UpdateSelection(
                    row.RowIndex,
                    select: true,
                    rangeModifier: e.KeyModifiers.HasFlag(KeyModifiers.Shift),
                    toggleModifier: e.KeyModifiers.HasFlag(KeyModifiers.Control),
                    rightButton: point.Properties.IsRightButtonPressed);
                e.Handled = true;
            }
        }

        private TreeDataGridRow? GetFocusedRow()
        {
            var focus = FocusManager.Instance;
            TreeDataGridRow? focused = null;
            if (focus.Current is IControl current)
                TryGetRow(current, out focused);
            return focused;
        }

        private bool MoveSelection(NavigationDirection direction, bool rangeModifier, TreeDataGridRow? focused)
        {
            if (Source is null || RowsPresenter is null || Source.Columns.Count == 0 || Source.Rows.Count == 0)
                return false;

            var currentRowIndex = focused?.RowIndex ?? Selection?.SelectedIndex ?? 0;
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

                newRowIndex = Math.Max(0, Math.Min(currentRowIndex + step.y, Source.Rows.Count - 1));
            }

            if (newRowIndex != currentRowIndex)
                UpdateSelection(newRowIndex, true, rangeModifier);

            if (newRowIndex != currentRowIndex)
            {
                RowsPresenter?.BringIntoView(newRowIndex);
                TryGetRow(newRowIndex)?.Focus();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryKeyExpandCollapse(NavigationDirection direction, TreeDataGridRow focused)
        {
            if (Source is null || RowsPresenter is null || focused.RowIndex < 0)
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

        private bool TryGetRow(IControl? element, [MaybeNullWhen(false)] out TreeDataGridRow result)
        {
            if (element is TreeDataGridRow row && row.RowIndex >= 0)
            {
                result = row;
                return true;
            }

            do
            {
                result = element?.FindAncestorOfType<TreeDataGridRow>();
                if (result?.RowIndex >= 0)
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
                if (_selection.IsSelected(index) && !SelectionCanceled())
                    _selection.Deselect(index);
            }
            else if (rightButton)
            {
                if (_selection.IsSelected(index) == false && !SelectionCanceled())
                {
                    _selection.SelectedIndex = index;
                }
            }
            else if (range)
            {
                if (!SelectionCanceled())
                {
                    using var operation = _selection.BatchUpdate();
                    _selection.Clear();
                    _selection.SelectRange(_selection.AnchorIndex, index);
                }
            }
            else if (multi && toggle)
            {
                if (!SelectionCanceled())
                {
                    if (_selection.IsSelected(index) == true)
                        _selection.Deselect(index);
                    else
                        _selection.Select(index);
                }
            }
            else if (toggle)
            {
                if (!SelectionCanceled())
                    _selection.SelectedIndex = (_selection.SelectedIndex == index) ? -1 : index;
            }
            else if (_selection.SelectedIndex != index || _selection.Count > 1)
            {
                if (!SelectionCanceled())
                    _selection.SelectedIndex = index;
            }
        }

        private bool SelectionCanceled()
        {
            if (SelectionChanging is null)
                return false;
            var e = new CancelEventArgs();
            SelectionChanging(this, e);
            return e.Cancel;
        }

        internal void RaiseCellClearing(TreeDataGridCell cell, int columnIndex, int rowIndex)
        {
            if (CellClearing is object)
            {
                _cellArgs ??= new TreeDataGridCellEventArgs();
                _cellArgs.Update(cell, columnIndex, rowIndex);
                CellClearing(this, _cellArgs);
                _cellArgs.Update(null, -1, -1);
            }
        }

        internal void RaiseCellPrepared(TreeDataGridCell cell, int columnIndex, int rowIndex)
        {
            if (CellPrepared is object)
            {
                _cellArgs ??= new TreeDataGridCellEventArgs();
                _cellArgs.Update(cell, columnIndex, rowIndex);
                CellPrepared(this, _cellArgs);
                _cellArgs.Update(null, -1, -1);
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (_source is object &&
                e.Source is TreeDataGridColumnHeader columnHeader &&
                columnHeader.ColumnIndex >= 0 &&
                columnHeader.ColumnIndex < _source.Columns.Count &&
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

                var column = _source.Columns[columnHeader.ColumnIndex];
                _source.SortBy(column, _userSortDirection, _selection!);
            }
        }
    }
}
