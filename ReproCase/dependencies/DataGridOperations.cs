using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

namespace UiAvalonia.Table
{
    internal static class DataGridOperations
    {
        internal static T GetNodeAtRow<T>(
            TreeDataGrid dataGrid, int targetRow)
        {
            return ((IRow<T>)dataGrid.Rows[targetRow]).Model;
        }

        internal static List<T> GetSelectedNodes<T>(
            TreeDataGrid dataGrid)
        {
            List<T> result = new List<T>();

            if (dataGrid.Selection == null)
                return result;

            foreach (IRow<T> row in dataGrid.Selection.SelectedItems)
                result.Add(row.Model);

            return result;
        }

        internal static int GetFirstSelectedRow(TreeDataGrid dataGrid)
        {
            if (dataGrid.Selection.Count == 0)
                return -1;

            return dataGrid.Selection.SelectedIndexes.First();
        }

        internal static void SelectDefaultRow(
            TreeDataGrid dataGrid, int defaultRow)
        {
            if (!CanSelectDefaultRow(dataGrid, defaultRow))
                return;

            int rowCount = dataGrid.Source.Rows.Count;

            if (defaultRow >= rowCount)
                defaultRow = rowCount - 1;

            dataGrid.Selection.Select(defaultRow);
            //dataGrid.RowsPresenter.BringIntoView(defaultRow);
        }

        static bool CanSelectDefaultRow(
            TreeDataGrid dataGrid, int defaultRow)
        {
            return defaultRow >= 0 && dataGrid.Source.Rows.Count != 0;
        }
    }
}