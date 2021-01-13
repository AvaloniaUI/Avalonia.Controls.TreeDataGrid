﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    /// <summary>
    /// A data source for a <see cref="TreeDataGrid"/> which displays a hierarchial tree where each
    /// row may have multiple columns.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class HierarchicalTreeDataGridSource<TModel> : ITreeDataGridSource,
        IExpanderRowController<TModel>
        where TModel : class
    {
        private IExpanderColumn<TModel>? _expanderColumn;
        private HierarchicalRows<TModel>? _rows;
        private CellList? _cells;
        private Comparison<TModel>? _comparison;

        public HierarchicalTreeDataGridSource(TModel root)
            : this(new[] { root })
        {
        }

        public HierarchicalTreeDataGridSource(IEnumerable<TModel> roots)
        {
            RootModels = ItemsSourceView<TModel>.GetOrCreate(roots);
            Columns = new ColumnList<TModel>();
            Columns.CollectionChanged += OnColumnsCollectionChanged;
        }

        public ItemsSourceView<TModel> RootModels { get; }
        public ICells Cells => _cells ??= CreateCells();
        public IRows Rows => _rows ??= CreateRows();
        public ColumnList<TModel> Columns { get; }
        IColumns ITreeDataGridSource.Columns => Columns;

        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowExpanding;
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowExpanded;
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowCollapsing;
        public event EventHandler<RowEventArgs<HierarchicalRow<TModel>>>? RowCollapsed;

        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;
            _rows?.Sort(_comparison);
        }

        void IExpanderRowController<TModel>.OnBeginExpandCollapse(IExpanderRow<TModel> row)
        {
            if (row is HierarchicalRow<TModel> r)
            {
                if (!row.IsExpanded)
                    RowExpanding?.Invoke(this, RowEventArgs.Create(r));
                else
                    RowCollapsing?.Invoke(this, RowEventArgs.Create(r));
            }
        }

        void IExpanderRowController<TModel>.OnEndExpandCollapse(IExpanderRow<TModel> row, bool oldValue)
        {
            if (row is HierarchicalRow<TModel> r && r.IsExpanded != oldValue)
            {
                if (row.IsExpanded)
                    RowExpanded?.Invoke(this, RowEventArgs.Create(r));
                else
                    RowCollapsed?.Invoke(this, RowEventArgs.Create(r));
            }
        }

        void IExpanderRowController<TModel>.OnChildCollectionChanged(
            IExpanderRow<TModel> row,
            NotifyCollectionChangedEventArgs e)
        {
        }

        bool ITreeDataGridSource.SortBy(IColumn? column, ListSortDirection direction)
        {
            if (column is IColumn<TModel> columnBase &&
                Columns.Contains(columnBase) &&
                columnBase.GetComparison(direction) is Comparison<TModel> comparison)
            {
                Sort(comparison);
                foreach (var c in Columns)
                    c.SortDirection = c == column ? (ListSortDirection?)direction : null;
                return true;
            }

            return false;
        }

        internal TModel GetModelAt(IndexPath index)
        {
            if (_expanderColumn is null)
                throw new InvalidOperationException("No expander column defined.");

            var items = (IEnumerable<TModel>?)RootModels;
            var count = index.GetSize();

            for (var depth = 0; depth < count - 1; ++depth)
            {
                items = _expanderColumn.GetChildModels(items.ElementAt(index.GetAt(depth)));
            }

            return items.ElementAt(index.GetLeaf()!.Value);
        }

        internal int GetRowIndex(IndexPath index, int fromRowIndex = 0) =>
            _rows?.GetRowIndex(index, fromRowIndex) ?? -1;

        private HierarchicalRows<TModel> CreateRows()
        {
            if (Columns.Count == 0)
                throw new InvalidOperationException("No columns defined.");
            if (_expanderColumn is null)
                throw new InvalidOperationException("No expander column defined.");

            var result = new HierarchicalRows<TModel>(this, RootModels, _expanderColumn, _comparison);
            result.CollectionChanged += OnRowsCollectionChanged;
            return result;
        }

        private CellList CreateCells()
        {
            var result = new CellList(Columns.Count);
            Reset(result);
            return result;
        }

        private ICell CreateCell(HierarchicalRow<TModel> row, int columnIndex)
        {
            return Columns[columnIndex].CreateCell(row);
        }

        private void Reset(CellList cells)
        {
            _rows ??= CreateRows();
            cells.Clear();

            foreach (var row in _rows)
            {
                var columnCount = Columns.Count;

                for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                    cells.Add(CreateCell(row, columnIndex));
            }
        }

        private void OnColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (_expanderColumn is null)
                    {
                        foreach (var i in e.NewItems)
                        {
                            if (i is IExpanderColumn<TModel> expander)
                            {
                                _expanderColumn = expander;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void OnRowsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_cells is null)
                return;

            void Add(int rowIndex, IList rows)
            {
                var cellIndex = rowIndex * Columns.Count;
                var columnCount = Columns.Count;

                foreach (HierarchicalRow<TModel> row in rows)
                {
                    for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                        _cells.Insert(cellIndex++, CreateCell(row, columnIndex));
                }
            }

            void Remove(int rowIndex, int rowCount)
            {
                _cells.RemoveRange(rowIndex * Columns.Count, rowCount * Columns.Count);
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var cellIndex = e.NewStartingIndex * Columns.Count;
                    var columnCount = Columns.Count;

                    foreach (HierarchicalRow<TModel> row in e.NewItems)
                    {
                        for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                            _cells[cellIndex++] = CreateCell(row, columnIndex);
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    Remove(e.OldStartingIndex, e.OldItems.Count);
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset(_cells);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}