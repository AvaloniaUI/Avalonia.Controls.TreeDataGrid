using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Avalonia;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;

using PlasticGui;

namespace UiAvalonia.Table
{
    internal delegate bool AreSameObjectFunc<IPlasticTreeNode>(
        IPlasticTreeNode a, IPlasticTreeNode b);

    public interface IPlasticTree
    {
        int GetNodesCount();
        List<IPlasticTreeNode> GetNodes();
        void Filter(Filter filter, List<string> columnNames);
    }

    public interface IPlasticTreeNode
    {
        int GetChildrenCount();
        IPlasticTreeNode GetParent();
        List<IPlasticTreeNode> GetChildren();
    }

    public interface IFilterableTable
    {
        void ApplyFilter(Filter filter);
    }

    internal class PlasticTree<T> : IFilterableTable
        where T : IPlasticTreeNode
    {
        internal TreeDataGrid Tree { get { return mTree; } }

        internal PlasticTree(
            PlasticTableColumn[] columns,
            Func<string, IPlasticTreeNode, PlasticTableCell> cellFunc,
            Func<IPlasticTree, IPlasticTreeNode, bool> expandFunc,
            Func<IPlasticTreeNode, bool> defaultSelectionFunc,
            AreSameObjectFunc<IPlasticTreeNode> sameObjectFunc,
            Dictionary<string, IComparer<T>> sorters,
            string defaultSortColumnName,
            bool defaultAscendingSort,
            string serializationName,
            Thickness borderThickness,
            Action afterSourceChangedAction = null)
        {
            mColumns = columns;
            mCellFunc = cellFunc;
            mExpandFunc = expandFunc;
            mDefaultSelectionFunc = defaultSelectionFunc;
            mSameObjectFunc = sameObjectFunc;
            mAfterItemsChangedAction = afterSourceChangedAction;

            mColumnNames = PlasticTableColumn.GetColumnNames(mColumns);

            mTree = new TreeDataGrid();

            mSource = BuildTreeSource(
                columns, cellFunc, sorters,
                defaultSortColumnName, defaultAscendingSort);

            SelectionModel<IRow> selection = new SelectionModel<IRow>(mSource.Rows)
            {
                SingleSelect = false,
            };

            mTree.BeginInit();
            mTree.Source = mSource;
            mTree.Selection = selection;
            mTree.EndInit();

            mTree.CellPrepared += Tree_CellPrepared;
        }

        internal void Dispose()
        {
            mSource.Dispose();
        }

        internal List<T> GetSelection()
        {
            return TreeOperations.GetSelectedNodesOfType<T>(mTree);
        }

        internal void Fill(
            IPlasticTree treeModel,
            List<IPlasticTreeNode> itemsToSelect,
            Filter currentFilter = null)
        {
            mTreeModel = treeModel;

            FillItemsAndSelectRows(
                treeModel, itemsToSelect,
                mExpandFunc, currentFilter);
        }

        void IFilterableTable.ApplyFilter(Filter filter)
        {
            FillItemsAndSelectRows(
                mTreeModel, null, mExpandFunc, filter);
        }

        void Tree_CellPrepared(object sender, TreeDataGridCellEventArgs e)
        {
            IPlasticTreeNode node = DataGridOperations.
                GetNodeAtRow<IPlasticTreeNode>(mTree, e.RowIndex);

            PlasticTableColumn column = mColumns[e.ColumnIndex];
            PlasticTableCell cell = mCellFunc(column.Name, node);

            PlasticTableCellBuilder.UpdateCellControl(
                column.RenderType, cell, e.Cell, node);
        }

        void FillItemsAndSelectRows(
            IPlasticTree treeModel,
            List<IPlasticTreeNode> itemsToSelect,
            Func<IPlasticTree, IPlasticTreeNode, bool> expandNodesFunc,
            Filter currentFilter)
        {
            if (itemsToSelect == null || itemsToSelect.Count == 0)
            {
                itemsToSelect = DataGridOperations.
                    GetSelectedNodes<IPlasticTreeNode>(mTree);
            }

            int defaultRow = DataGridOperations.GetFirstSelectedRow(mTree);

            if (currentFilter != null)
                treeModel.Filter(currentFilter, mColumnNames);

            mSource.Items = treeModel.GetNodes();

            if (mAfterItemsChangedAction != null)
                mAfterItemsChangedAction();

            ExpandItems(treeModel, mSource, expandNodesFunc);

            SetSelectedRows(
                mTree, mDefaultSelectionFunc,
                itemsToSelect, defaultRow, mSameObjectFunc);
        }

        HierarchicalTreeDataGridSource<IPlasticTreeNode> BuildTreeSource(
            PlasticTableColumn[] columns,
            Func<string, IPlasticTreeNode, PlasticTableCell> cellFunc,
            Dictionary<string, IComparer<T>> sorters,
            string defaultSortColumnName,
            bool defaultAscendingSort)
        {
            HierarchicalTreeDataGridSource<IPlasticTreeNode> result =
                new HierarchicalTreeDataGridSource<IPlasticTreeNode>(new List<IPlasticTreeNode>());

            IColumn<IPlasticTreeNode> defaultSortTemplateColumn = null;

            for (int columnIndex = 0; columnIndex < columns.Length; columnIndex++)
            {
                PlasticTableColumn column = columns[columnIndex];

                IComparer<T> comparer;
                sorters.TryGetValue(column.Name, out comparer);

                IColumn<IPlasticTreeNode> templateColumn = BuildTemplateColumn(
                    column, columnIndex == 0, cellFunc, comparer);

                result.Columns.Add(templateColumn);

                if (column.Name == defaultSortColumnName)
                    defaultSortTemplateColumn = templateColumn;
            }

            ListSortDirection sortDirection = defaultAscendingSort ?
                ListSortDirection.Ascending : ListSortDirection.Descending;

            result.SortBy(defaultSortTemplateColumn, sortDirection, null);

            defaultSortTemplateColumn.SortDirection = sortDirection;

            return result;
        }

        IColumn<IPlasticTreeNode> BuildTemplateColumn(
            PlasticTableColumn column,
            bool isExpanderColumn,
            Func<string, IPlasticTreeNode, PlasticTableCell> cellFunc,
            IComparer<T> comparer)
        {
            if (isExpanderColumn)
            {
                return new HierarchicalExpanderColumn<IPlasticTreeNode>(
                    new TemplateColumn<IPlasticTreeNode>(
                        column.Content != null ? column.Content : column.Name,
                        new FuncDataTemplate<IPlasticTreeNode>((IPlasticTreeNode node, INameScope ns) =>
                        {
                            return BuildCellControl(column, cellFunc, node);
                        }, true),
                        new GridLength(column.DefaultWidth, GridUnitType.Pixel),
                        new ColumnOptions<IPlasticTreeNode>
                        {
                            CompareAscending = Sort(comparer, false),
                            CompareDescending = Sort(comparer, true),
                        }),
                    x => x.GetChildren(),
                    x => x.GetChildrenCount() > 0);
            }

            return new TemplateColumn<IPlasticTreeNode>(
                column.Name,
                new FuncDataTemplate<IPlasticTreeNode>((IPlasticTreeNode node, INameScope ns) =>
                {
                    return BuildCellControl(column, cellFunc, node);
                }, true),
                new GridLength(column.DefaultWidth, GridUnitType.Pixel),
                new ColumnOptions<IPlasticTreeNode>
                {
                    CompareAscending = Sort(comparer, false),
                    CompareDescending = Sort(comparer, true),
                });
        }

        IControl BuildCellControl(
            PlasticTableColumn column,
            Func<string, IPlasticTreeNode, PlasticTableCell> cellFunc,
            IPlasticTreeNode node)
        {
            PlasticTableCell cell = cellFunc(column.Name, node);

            return PlasticTableCellBuilder.CreateCellControl(
                column.RenderType, cell, node);
        }

        static Comparison<IPlasticTreeNode> Sort(
            IComparer<T> comparer, bool bDescending)
        {
            return (IPlasticTreeNode x, IPlasticTreeNode y) =>
            {
                if (!(x is T) || !(y is T))
                    return 0;

                int result = -1;// comparer.Compare((T)x, (T)y);
                return bDescending ? -result : result;
            };
        }

        static void SetSelectedRows(
            TreeDataGrid tree,
            Func<IPlasticTreeNode, bool> defaultSelectionFunc,
            List<IPlasticTreeNode> itemsToSelect,
            int defaultRow,
            AreSameObjectFunc<IPlasticTreeNode> sameObjectFunc)
        {
            if (itemsToSelect.Count == 0)
            {
                if (defaultSelectionFunc == null)
                    return;

                defaultRow = TreeOperations.GetFirstIndexThatMatches(
                    tree, defaultSelectionFunc);

                if (defaultRow == -1)
                    return;

                DataGridOperations.SelectDefaultRow(tree, defaultRow);
                return;
            }

            foreach (IPlasticTreeNode item in itemsToSelect)
            {
                int index = TreeOperations.FindRow(
                    tree, item, sameObjectFunc);

                if (index == -1)
                    continue;

                tree.Selection.Select(index);
            }

            if (tree.Selection.Count > 0)
            {
                //tree.RowsPresenter.BringIntoView(
                //    DataGridOperations.GetFirstSelectedRow(tree));
                return;
            }

            defaultRow = defaultSelectionFunc != null ?
                TreeOperations.GetFirstIndexThatMatches(tree, defaultSelectionFunc) :
                defaultRow;

            if (defaultRow == -1)
                return;

            DataGridOperations.SelectDefaultRow(tree, defaultRow);
        }

        static void ExpandItems(
            IPlasticTree treeModel,
            HierarchicalTreeDataGridSource<IPlasticTreeNode> source,
            Func<IPlasticTree, IPlasticTreeNode, bool> expandNodesFunc)
        {
            int index = 0;

            foreach (IPlasticTreeNode node in source.Items)
            {
                if (expandNodesFunc(treeModel, node))
                    source.Expand(new IndexPath(index));

                int childIndex = 0;

                foreach (IPlasticTreeNode childNode in node.GetChildren())
                {
                    if (childNode.GetChildrenCount() == 0)
                        continue;

                    if (expandNodesFunc(treeModel, childNode))
                        source.Expand(new IndexPath(index, childIndex));

                    childIndex++;
                }

                index++;
            }
        }

        IPlasticTree mTreeModel;

        readonly HierarchicalTreeDataGridSource<IPlasticTreeNode> mSource;
        readonly TreeDataGrid mTree;
        readonly Action mAfterItemsChangedAction;
        readonly AreSameObjectFunc<IPlasticTreeNode> mSameObjectFunc;
        readonly Func<IPlasticTreeNode, bool> mDefaultSelectionFunc;
        readonly Func<IPlasticTree, IPlasticTreeNode, bool> mExpandFunc;
        readonly Func<string, IPlasticTreeNode, PlasticTableCell> mCellFunc;
        readonly PlasticTableColumn[] mColumns;
        readonly List<string> mColumnNames;
    }

    internal static class TreeOperations
    {
        internal static Func<IPlasticTree, IPlasticTreeNode, bool> BuildExpandAllFunction()
        {
            return (IPlasticTree tree, IPlasticTreeNode node) =>
            {
                return true;
            };
        }

        internal static List<T> GetSelectedNodesOfType<T>(
            TreeDataGrid tree)
        {
            List<T> result = new List<T>();

            if (tree.Selection == null)
                return result;

            foreach (object selectedItem in tree.Selection.SelectedItems)
            {
                HierarchicalRow<IPlasticTreeNode> row =
                    (HierarchicalRow<IPlasticTreeNode>)selectedItem;

                if (row.Model is T)
                {
                    result.Add((T)row.Model);
                }
            }

            return result;
        }

        internal static IEnumerable<IPlasticTreeNode> GetSourceItems(
            TreeDataGrid tree)
        {
            return ((HierarchicalTreeDataGridSource<IPlasticTreeNode>)tree.Source).Items;
        }

        internal static IPlasticTreeNode GetFirstItemThatMatches(
            TreeDataGrid tree, Func<IPlasticTreeNode, bool> func)
        {
            foreach (HierarchicalRow<IPlasticTreeNode> row in tree.Source.Rows)
            {
                if (func(row.Model))
                    return row.Model;
            }

            return null;
        }

        internal static int GetFirstIndexThatMatches(
            TreeDataGrid tree,
            Func<IPlasticTreeNode, bool> func)
        {
            for (int i = 0; i < tree.Source.Rows.Count; i++)
            {
                HierarchicalRow<IPlasticTreeNode> row =
                    (HierarchicalRow<IPlasticTreeNode>)tree.Source.Rows[i];

                if (!func(row.Model))
                    continue;

                return i;
            }

            return -1;
        }

        internal static void Foreach(
            TreeDataGrid tree,
            Func<IPlasticTreeNode, bool> func)
        {
            for (int i = 0; i < tree.Source.Rows.Count; i++)
            {
                HierarchicalRow<IPlasticTreeNode> row =
                    (HierarchicalRow<IPlasticTreeNode>)tree.Source.Rows[i];

                if (func(row.Model))
                    return;
            }
        }

        internal static int FindRow(
            TreeDataGrid tree,
            IPlasticTreeNode target)
        {
            for (int i = 0; i < tree.Source.Rows.Count; i++)
            {
                IPlasticTreeNode node = DataGridOperations.
                    GetNodeAtRow<IPlasticTreeNode>(tree, i);

                if (node == target)
                    return i;
            }

            return -1;
        }

        internal static int FindRow(
            TreeDataGrid tree,
            IPlasticTreeNode target,
            AreSameObjectFunc<IPlasticTreeNode> sameObjectFunc)
        {
            for (int i = 0; i < tree.Source.Rows.Count; i++)
            {
                IPlasticTreeNode node = DataGridOperations.
                    GetNodeAtRow<IPlasticTreeNode>(tree, i);

                bool areEquals = sameObjectFunc == null ?
                    node == target :
                    sameObjectFunc(node, target);

                if (areEquals)
                    return i;
            }

            return -1;
        }

        internal static void Expand(
            TreeDataGrid tree,
            IPlasticTreeNode target)
        {
            int targetRow = FindRow(tree, target);

            if (targetRow == -1)
                return;

            ((HierarchicalTreeDataGridSource<IPlasticTreeNode>)tree.Source).
                Expand(new IndexPath(targetRow));
        }
    }
}