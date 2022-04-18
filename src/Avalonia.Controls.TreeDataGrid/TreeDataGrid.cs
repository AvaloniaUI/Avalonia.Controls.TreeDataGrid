using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

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
                o => o.ElementFactory,
                (o, v) => o.ElementFactory = v);

        public static readonly DirectProperty<TreeDataGrid, IRows?> RowsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IRows?>(
                nameof(Rows),
                o => o.Rows,
                (o, v) => o.Rows = v);

        [Browsable(false)]
        public static readonly DirectProperty<TreeDataGrid, ITreeDataGridSelectionInteraction?> SelectionInteractionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, ITreeDataGridSelectionInteraction?>(
                nameof(SelectionInteraction),
                o => o.SelectionInteraction,
                (o, v) => o.SelectionInteraction = v);

        public static readonly DirectProperty<TreeDataGrid, IScrollable?> ScrollProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IScrollable?>(
                nameof(Scroll),
                o => o.Scroll);

        public static readonly StyledProperty<bool> ShowColumnHeadersProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(ShowColumnHeaders), true);

        public static readonly DirectProperty<TreeDataGrid, ITreeDataGridSource?> SourceProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, ITreeDataGridSource?>(
                nameof(Source),
                o => o.Source,
                (o, v) => o.Source = v);

        private IElementFactory? _elementFactory;
        private ITreeDataGridSource? _source;
        private IColumns? _columns;
        private IRows? _rows;
        private IScrollable? _scroll;
        private ITreeDataGridSelectionInteraction? _selection;
        private IControl? _userSortColumn;
        private ListSortDirection _userSortDirection;
        private TreeDataGridCellEventArgs? _cellArgs;

        public TreeDataGrid()
        {
            AddHandler(TreeDataGridColumnHeader.ClickEvent, OnClick);
            AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
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

        public IElementFactory ElementFactory
        {
            get => _elementFactory ??= CreateDefaultElementFactory();
            set
            {
                _ = value ?? throw new ArgumentNullException(nameof(value));
                SetAndRaise(ElementFactoryProperty, ref _elementFactory!, value);
            }
        }

        public IRows? Rows
        {
            get => _rows;
            private set => SetAndRaise(RowsProperty, ref _rows, value);
        }

        [Browsable(false)]
        public ITreeDataGridSelectionInteraction? SelectionInteraction
        {
            get => _selection;
            private set => SetAndRaise(SelectionInteractionProperty, ref _selection, value);
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

        public ITreeSelectionModel? RowSelection => Source?.Selection as ITreeSelectionModel;

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

                    if (_source != null)
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
                    SelectionInteraction = value?.Selection as ITreeDataGridSelectionInteraction;
                    RaisePropertyChanged(
                        SourceProperty,
                        new Optional<ITreeDataGridSource?>(oldSource),
                        new BindingValue<ITreeDataGridSource?>(oldSource));
                }
            }
        }

        public event EventHandler<TreeDataGridCellEventArgs>? CellClearing;
        public event EventHandler<TreeDataGridCellEventArgs>? CellPrepared;
        public event CancelEventHandler? SelectionChanging;

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

        public bool TryGetRow(IControl? element, [MaybeNullWhen(false)] out TreeDataGridRow result)
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
            } while (result is not null);

            return result is not null;
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

        public bool QueryCancelSelection()
        {
            if (SelectionChanging is null)
                return false;
            var e = new CancelEventArgs();
            SelectionChanging(this, e);
            return e.Cancel;
        }

        protected virtual IElementFactory CreateDefaultElementFactory() => new TreeDataGridElementFactory();

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            ColumnHeadersPresenter = e.NameScope.Find<TreeDataGridColumnHeadersPresenter>("PART_ColumnHeadersPresenter");
            RowsPresenter = e.NameScope.Find<TreeDataGridRowsPresenter>("PART_RowsPresenter");
            Scroll = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        }

        protected void OnPreviewKeyDown(object? o, KeyEventArgs e)
        {
            if (e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                e.Handled = true;
            }
            
            _selection?.OnPreviewKeyDown(this, e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _selection?.OnKeyDown(this, e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _selection?.OnKeyUp(this, e);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            _selection?.OnTextInput(this, e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            _selection?.OnPointerPressed(this, e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            _selection?.OnPointerMoved(this, e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            _selection?.OnPointerReleased(this, e);
        }

        internal void RaiseCellClearing(TreeDataGridCell cell, int columnIndex, int rowIndex)
        {
            if (CellClearing is not null)
            {
                _cellArgs ??= new TreeDataGridCellEventArgs();
                _cellArgs.Update(cell, columnIndex, rowIndex);
                CellClearing(this, _cellArgs);
                _cellArgs.Update(null, -1, -1);
            }
        }

        internal void RaiseCellPrepared(TreeDataGridCell cell, int columnIndex, int rowIndex)
        {
            if (CellPrepared is not null)
            {
                _cellArgs ??= new TreeDataGridCellEventArgs();
                _cellArgs.Update(cell, columnIndex, rowIndex);
                CellPrepared(this, _cellArgs);
                _cellArgs.Update(null, -1, -1);
            }
        }

        private void OnClick(object? sender, RoutedEventArgs e)
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
                _source.SortBy(column, _userSortDirection);
            }
        }
    }
}
