using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Layout;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridLayout : VirtualizingLayout
    {
        // HACK: Using a fixed row height for now.
        private const double RowHeight = 25;

        public static readonly DirectProperty<TreeDataGridLayout, IReadOnlyList<IColumn>?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridLayout, IReadOnlyList<IColumn>?>(
                nameof(Columns),
                o => o.Columns,
                (o, v) => o.Columns = v);

        public static readonly DirectProperty<TreeDataGridLayout, SharedLayoutState?> SharedStateProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridLayout, SharedLayoutState?>(
                nameof(SharedState),
                o => o.SharedState,
                (o, v) => o.SharedState = v);

        private IReadOnlyList<IColumn>? _columns;

        public IReadOnlyList<IColumn>? Columns
        {
            get => _columns;
            set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        public SharedLayoutState? SharedState { get; set; }

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.InitializeForContextCore(context);
            context.LayoutState ??= new TreeListLayoutState();
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.UninitializeForContextCore(context);
            context.LayoutState = null;
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            if (_columns is null || _columns.Count == 0 || SharedState is null)
                return default;

            var columnCount = _columns.Count;
            var rowCount = context.ItemCount / columnCount;
            var state = (TreeListLayoutState)context.LayoutState;
            var minWidth = 0.0;
            var remainingWidth = availableSize.Width;
            var totalStars = 0.0;

            state.FirstRealizedRow = Math.Max(
                (int)(context.RealizationRect.Y / RowHeight) - 1,
                0);
            state.LastRealizedRow = Math.Min(
                (int)(context.RealizationRect.Bottom / RowHeight) + 1,
                (context.ItemCount - columnCount) / columnCount);

            if (!(SharedState.ColumnWidths?.Length == columnCount))
            {
                SharedState.ColumnWidths = new double[columnCount];
            }

            for (var pass = GridUnitType.Auto; pass <= GridUnitType.Star; ++pass)
            {
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var column = _columns[columnIndex];

                    if (pass == GridUnitType.Auto && column.Width.IsStar)
                        totalStars += column.Width.Value;

                    if (!(column.Width.GridUnitType == pass))
                        continue;

                    var availableWidth = pass switch
                    {
                        GridUnitType.Auto => availableSize.Width,
                        GridUnitType.Pixel => column.Width.Value,
                        GridUnitType.Star => remainingWidth * (column.Width.Value / totalStars),
                        _ => 0,
                    };

                    var minColumnWidth = 0.0;

                    for (var rowIndex = state.FirstRealizedRow; rowIndex <= state.LastRealizedRow; rowIndex++)
                    {
                        int firstItemIndex = rowIndex * 3;
                        var index = firstItemIndex + columnIndex;
                        var container = context.GetOrCreateElementAt(index);

                        container.Measure(new Size(availableWidth, RowHeight));
                        minColumnWidth = Math.Max(minColumnWidth, container.DesiredSize.Width);
                    }

                    SharedState.ColumnWidths[columnIndex] = pass switch
                    {
                        GridUnitType.Auto => Math.Max(minColumnWidth, SharedState.ColumnWidths[columnIndex]),
                        GridUnitType.Pixel => column.Width.Value,
                        GridUnitType.Star => minColumnWidth,
                        _ => 0,
                    };

                    if (pass != GridUnitType.Star)
                        minWidth += SharedState.ColumnWidths[columnIndex];

                    remainingWidth -= minColumnWidth;
                }
            }

            return new Size(minWidth, rowCount * RowHeight);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            if (_columns is null || _columns.Count == 0 || SharedState is null)
                return default;

            var state = (TreeListLayoutState)context.LayoutState;
            var columnCount = _columns.Count;
            var remainingWidth = finalSize.Width;
            var totalStars = 0.0;
            var x = 0.0;

            for (var i = 0; i < _columns.Count; ++i)
            {
                var column = _columns[i];
                if (column.Width.GridUnitType == GridUnitType.Star)
                    totalStars += column.Width.Value;
                else
                    remainingWidth -= SharedState.ColumnWidths![i];
            }

            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                var column = _columns[columnIndex];
                var columnWidth = Math.Max(
                    column.Width.GridUnitType != GridUnitType.Star ?
                        SharedState.ColumnWidths![columnIndex] :
                        remainingWidth * (column.Width.Value / totalStars),
                    0);

                for (int rowIndex = state.FirstRealizedRow; rowIndex <= state.LastRealizedRow; rowIndex++)
                {
                    var index = (rowIndex * columnCount) + columnIndex;
                    var container = context.GetOrCreateElementAt(index);
                    var rect = new Rect(x, rowIndex * RowHeight, columnWidth, RowHeight);
                    container.Arrange(rect);
                }

                x += columnWidth;
                SharedState.ColumnWidths![columnIndex] = columnWidth;
            }

            return finalSize;
        }

        private class TreeListLayoutState
        {
            public int FirstRealizedRow { get; set; }
            public int LastRealizedRow { get; set; }
        }

        public class SharedLayoutState
        {
            public double[]? ColumnWidths { get; set; }
        }
    }
}
