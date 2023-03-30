﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace Avalonia.Controls
{
    public class TreeDataGrid : TemplatedControl
    {
        public static readonly StyledProperty<bool> AutoDragDropRowsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(AutoDragDropRows));

        public static readonly StyledProperty<bool> CanUserResizeColumnsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserResizeColumns), true);

        public static readonly StyledProperty<bool> CanUserSortColumnsProperty =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(CanUserSortColumns), true);

        public static readonly DirectProperty<TreeDataGrid, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, IColumns?>(
                nameof(Columns),
                o => o.Columns);

        public static readonly DirectProperty<TreeDataGrid, TreeDataGridElementFactory> ElementFactoryProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGrid, TreeDataGridElementFactory>(
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

        public static readonly RoutedEvent<TreeDataGridRowDragStartedEventArgs> RowDragStartedEvent =
            RoutedEvent.Register<TreeDataGrid, TreeDataGridRowDragStartedEventArgs>(
                nameof(RowDragStarted),
                RoutingStrategies.Bubble);

        public static readonly RoutedEvent<TreeDataGridRowDragEventArgs> RowDragOverEvent =
            RoutedEvent.Register<TreeDataGrid, TreeDataGridRowDragEventArgs>(
                nameof(RowDragOver),
                RoutingStrategies.Bubble);

        public static readonly RoutedEvent<TreeDataGridRowDragEventArgs> RowDropEvent =
            RoutedEvent.Register<TreeDataGrid, TreeDataGridRowDragEventArgs>(
                nameof(RowDrop),
                RoutingStrategies.Bubble);

        private const double AutoScrollMargin = 60;
        private const int AutoScrollSpeed = 50;
        private TreeDataGridElementFactory? _elementFactory;
        private ITreeDataGridSource? _source;
        private IColumns? _columns;
        private IRows? _rows;
        private IScrollable? _scroll;
        private IScrollable? _headerScroll;
        private ITreeDataGridSelectionInteraction? _selection;
        private Control? _userSortColumn;
        private ListSortDirection _userSortDirection;
        private TreeDataGridCellEventArgs? _cellArgs;
        private Border? _dragAdorner;
        private DispatcherTimer? _autoScrollTimer;
        private bool _autoScrollDirection;

        public TreeDataGrid()
        {
            AddHandler(TreeDataGridColumnHeader.ClickEvent, OnClick);
            AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        }

        static TreeDataGrid()
        {
            DragDrop.DragOverEvent.AddClassHandler<TreeDataGrid>((x, e) => x.OnDragOver(e));
            DragDrop.DragLeaveEvent.AddClassHandler<TreeDataGrid>((x, e) => x.OnDragLeave(e));
            DragDrop.DropEvent.AddClassHandler<TreeDataGrid>((x, e) => x.OnDrop(e));
        }

        public bool AutoDragDropRows
        {
            get => GetValue(AutoDragDropRowsProperty);
            set => SetValue(AutoDragDropRowsProperty, value);
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

        public TreeDataGridElementFactory ElementFactory
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
                        oldSource,
                        oldSource);
                }
            }
        }

        public event EventHandler<TreeDataGridCellEventArgs>? CellClearing;
        public event EventHandler<TreeDataGridCellEventArgs>? CellPrepared;
        
        public event EventHandler<TreeDataGridRowDragStartedEventArgs>? RowDragStarted
        {
            add => AddHandler(RowDragStartedEvent, value!);
            remove => RemoveHandler(RowDragStartedEvent, value!);
        }

        public event EventHandler<TreeDataGridRowDragEventArgs>? RowDragOver
        {
            add => AddHandler(RowDragOverEvent, value!);
            remove => RemoveHandler(RowDragOverEvent, value!);
        }

        public event EventHandler<TreeDataGridRowDragEventArgs>? RowDrop
        {
            add => AddHandler(RowDropEvent, value!);
            remove => RemoveHandler(RowDropEvent, value!);
        }

        public event CancelEventHandler? SelectionChanging;

        public Control? TryGetCell(int columnIndex, int rowIndex)
        {
            if (TryGetRow(rowIndex) is TreeDataGridRow row &&
                row.TryGetCell(columnIndex) is Control cell)
            {
                return cell;
            }

            return null;
        }

        public TreeDataGridRow? TryGetRow(int rowIndex)
        {
            return RowsPresenter?.TryGetElement(rowIndex) as TreeDataGridRow;
        }

        public bool TryGetRow(Control? element, [MaybeNullWhen(false)] out TreeDataGridRow result)
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

        public bool TryGetRowModel<TModel>(Control element, [MaybeNullWhen(false)] out TModel result)
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

        protected virtual TreeDataGridElementFactory CreateDefaultElementFactory() => new TreeDataGridElementFactory();

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (Scroll is ScrollViewer s && _headerScroll is ScrollViewer h)
            {
                s.ScrollChanged -= OnScrollChanged;
                h.ScrollChanged -= OnHeaderScrollChanged;
            }

            base.OnApplyTemplate(e);
            ColumnHeadersPresenter = e.NameScope.Find<TreeDataGridColumnHeadersPresenter>("PART_ColumnHeadersPresenter");
            RowsPresenter = e.NameScope.Find<TreeDataGridRowsPresenter>("PART_RowsPresenter");
            Scroll = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
            _headerScroll = e.NameScope.Find<ScrollViewer>("PART_HeaderScrollViewer");

            if (Scroll is ScrollViewer s1 && _headerScroll is ScrollViewer h1)
            {
                s1.ScrollChanged += OnScrollChanged;
                h1.ScrollChanged += OnHeaderScrollChanged;
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            StopDrag();
        }

        protected void OnPreviewKeyDown(object? o, KeyEventArgs e)
        {         
            _selection?.OnPreviewKeyDown(this, e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == AutoDragDropRowsProperty)
            {
                DragDrop.SetAllowDrop(this, change.GetNewValue<bool>());
            }
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

        internal void RaiseRowDragStarted(PointerEventArgs trigger)
        {
            if (_source is null || RowSelection is null)
                return;

            var allowedEffects = AutoDragDropRows && !_source.IsSorted ? 
                DragDropEffects.Move : 
                DragDropEffects.None;
            var route = BuildEventRoute(RowDragStartedEvent);

            if (route.HasHandlers)
            {
                var e = new TreeDataGridRowDragStartedEventArgs(RowSelection.SelectedItems!);
                e.AllowedEffects = allowedEffects;
                RaiseEvent(e);
                allowedEffects = e.AllowedEffects;
            }

            if (allowedEffects != DragDropEffects.None)
            {
                var data = new DataObject();
                var info = new DragInfo(_source, RowSelection.SelectedIndexes.ToList());
                data.Set(DragInfo.DataFormat, info);
                DragDrop.DoDragDrop(trigger, data, allowedEffects);
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

        private Border? GetOrCreateDragAdorner()
        {
            if (_dragAdorner is not null)
                return _dragAdorner;

            var adornerLayer = AdornerLayer.GetAdornerLayer(this);

            if (adornerLayer is null)
                return null;

            _dragAdorner ??= new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                IsHitTestVisible = false,
                Margin = new Thickness(0, -1),
            };

            adornerLayer.Children.Add(_dragAdorner);
            AdornerLayer.SetIsClipEnabled(_dragAdorner, false);
            return _dragAdorner;
        }

        private void ShowDragAdorner(TreeDataGridRow row, TreeDataGridRowDropPosition position)
        {
            if (position == TreeDataGridRowDropPosition.None)
            {
                HideDragAdorner();
                return;
            }

            var adorner = GetOrCreateDragAdorner();
            if (adorner is null)
                return;

            AdornerLayer.SetAdornedElement(adorner, row);

            switch (position)
            {
                case TreeDataGridRowDropPosition.Before:
                    adorner.BorderThickness = new(0, 2, 0, 0);
                    break;
                case TreeDataGridRowDropPosition.After:
                    adorner.BorderThickness = new(0, 0, 0, 2);
                    break;
                case TreeDataGridRowDropPosition.Inside:
                    adorner.BorderThickness = new(2);
                    break;
            }
        }

        private void HideDragAdorner()
        {
            if (_dragAdorner is null)
                return;

            if (_dragAdorner.Parent is AdornerLayer layer)
                layer.Children.Remove(_dragAdorner);

            _dragAdorner = null;
        }

        private void StopDrag()
        {
            HideDragAdorner();
            _autoScrollTimer?.Stop();
        }

        private void AutoScroll(bool direction)
        {
            if (_autoScrollTimer is null)
            {
                _autoScrollTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(AutoScrollSpeed),
                };
                _autoScrollTimer.Tick += OnAutoScrollTick;
            }

            _autoScrollDirection = direction;

            if (!_autoScrollTimer.IsEnabled)
                OnAutoScrollTick(null, EventArgs.Empty);

            _autoScrollTimer.Start();
        }

        [MemberNotNullWhen(true, nameof(_source))]
        private bool CalculateAutoDragDrop(
            TreeDataGridRow targetRow,
            DragEventArgs e, 
            [NotNullWhen(true)] out DragInfo? data,
            out TreeDataGridRowDropPosition position)
        {
            if (!AutoDragDropRows ||
                e.Data.Get(DragInfo.DataFormat) is not DragInfo di ||
                _source is null ||
                _source.IsSorted ||
                di.Source != _source)
            {
                data = null;
                position = TreeDataGridRowDropPosition.None;
                return false;
            }

            var targetIndex = _source.Rows.RowIndexToModelIndex(targetRow.RowIndex);
            position = GetDropPosition(_source, e, targetRow);

            // We can't drop rows into themselves or their descendents.
            foreach (var sourceIndex in di.Indexes)
            {
                if (sourceIndex.IsAncestorOf(targetIndex) ||
                    (sourceIndex == targetIndex && position == TreeDataGridRowDropPosition.Inside))
                {
                    data = null;
                    position = TreeDataGridRowDropPosition.None;
                    return false;
                }
            }

            data = di;
            return true;
        }

        private void OnDragOver(DragEventArgs e)
        {
            if (!TryGetRow(e.Source as Control, out var row))
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            if (!CalculateAutoDragDrop(row, e, out _, out var adorner))
                e.DragEffects = DragDropEffects.None;

            var route = BuildEventRoute(RowDragOverEvent);

            if (route.HasHandlers)
            {
                var ev = new TreeDataGridRowDragEventArgs(RowDragOverEvent, row, e);
                ev.Position = adorner;
                RaiseEvent(ev);
                adorner = ev.Position;
            }

            ShowDragAdorner(row, adorner);

            if (Scroll is ScrollViewer scroller)
            {
                var rowsPosition = e.GetPosition(scroller);

                if (rowsPosition.Y < AutoScrollMargin)
                    AutoScroll(false);
                else if (rowsPosition.Y > Bounds.Height - AutoScrollMargin)
                    AutoScroll(true);
                else
                    _autoScrollTimer?.Stop();
            }
        }

        private void OnDragLeave(RoutedEventArgs e) => StopDrag();

        private void OnDrop(DragEventArgs e)
        {
            StopDrag();
            
            if (!TryGetRow(e.Source as Control, out var row))
                return;

            var autoDrop = CalculateAutoDragDrop(row, e, out var data, out var position);
            var route = BuildEventRoute(RowDropEvent);

            if (route.HasHandlers)
            {
                var ev = new TreeDataGridRowDragEventArgs(RowDropEvent, row, e);
                ev.Position = position;
                RaiseEvent(ev);

                if (ev.Handled || !e.DragEffects.HasAnyFlag(DragDropEffects.Move))
                    return;

                position = ev.Position;
            }

            if (autoDrop && 
                _source is not null &&
                position != TreeDataGridRowDropPosition.None)
            {
                var targetIndex = _source.Rows.RowIndexToModelIndex(row.RowIndex);
                _source.DragDropRows(_source, data!.Indexes, targetIndex, position, e.DragEffects);
            }
        }

        private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (Scroll is not null && _headerScroll is not null && !MathUtilities.IsZero(e.OffsetDelta.X))
                _headerScroll.Offset = _headerScroll.Offset.WithX(Scroll.Offset.X);
        }

        private void OnHeaderScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (Scroll is not null && _headerScroll is not null && !MathUtilities.IsZero(e.OffsetDelta.X))
                Scroll.Offset = Scroll.Offset.WithX(_headerScroll.Offset.X);
        }

        private void OnAutoScrollTick(object? sender, EventArgs e)
        {
            if (Scroll is ScrollViewer scroll)
            {
                if (!_autoScrollDirection)
                    scroll.LineUp();
                else
                    scroll.LineDown();
            }
        }

        private static TreeDataGridRowDropPosition GetDropPosition(
            ITreeDataGridSource source,
            DragEventArgs e,
            TreeDataGridRow row)
        {
            var rowY = e.GetPosition(row).Y / row.Bounds.Height;

            if (source.IsHierarchical)
            {
                if (rowY < 0.33)
                    return TreeDataGridRowDropPosition.Before;
                else if (rowY > 0.66)
                    return TreeDataGridRowDropPosition.After;
                else
                    return TreeDataGridRowDropPosition.Inside;
            }
            else
            {
                if (rowY < 0.5)
                    return TreeDataGridRowDropPosition.Before;
                else
                    return TreeDataGridRowDropPosition.After;
            }
        }
    }
}
