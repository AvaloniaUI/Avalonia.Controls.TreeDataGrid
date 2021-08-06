using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;

using UiAvalonia;
using UiAvalonia.Table;

using PlasticTableCell = UiAvalonia.Table.PlasticTableCell;

namespace PlasticAvalonia.WorkspaceWindow.Views.PendingChanges
{
    internal static class PendingChangesTreeDefinition
    {
        internal const string NAME = "PendingChangesTree";

        internal static PlasticTableColumn[] BuildColumns()
        {
            PlasticTableColumn[] result = new PlasticTableColumn[]
            {
                new PlasticTableColumn(
                    "ItemColumn",
                    ITEM_COLUMN_WIDTH,
                    PlasticTableColumn.Render.CheckBoxIconAndText),
                new PlasticTableColumn(
                    "StatusColumn",
                    STATUS_COLUMN_WIDTH,
                    PlasticTableColumn.Render.Text),
                new PlasticTableColumn(
                    "SizeColumn",
                    SIZE_COLUMN_WIDTH,
                    PlasticTableColumn.Render.Text),
                new PlasticTableColumn(
                    "DateModifiedColumn",
                    DATE_MODIFIED_COLUMN_WIDTH,
                    PlasticTableColumn.Render.Text),
                new PlasticTableColumn(
                    "ExtensionColumn",
                    EXTENSION_COLUMN_WIDTH,
                    PlasticTableColumn.Render.Text),
                new PlasticTableColumn(
                    "TypeColumn",
                    TYPE_COLUMN_WIDTH,
                    PlasticTableColumn.Render.Text),
                new PlasticTableColumn(
                    "SimilarityColumn",
                    SIMILARITY_COLUMN_WIDTH,
                    PlasticTableColumn.Render.Text),
                new PlasticTableColumn(
                    "RepositoryColumn",
                    REPOSITORY_COLUMN_WIDTH,
                    PlasticTableColumn.Render.Text)
            };

            return result;
        }

        internal static Dictionary<string, IComparer<PendingChangeInfo>> BuildColumnComparers()
        {
            Func<PendingChangeInfo, bool> isFastComparisonEnabled =
                (c) => ((PendingChangeCategory)c.GetParent()).GetChildrenCount() > 10000;

            return new Dictionary<string, IComparer<PendingChangeInfo>>();
        }

        internal static Func<string, IPlasticTreeNode, PlasticTableCell> BuildCellRenderFunction(
            CheckeablePlasticTableCell<IPlasticTreeNode>.OnCheckBoxClicked onCheckBoxClickedDelegate)
        {
            return (string columnName, IPlasticTreeNode node) =>
            {
                bool isItemColumn = columnName == "ItemColumn";

                if (isItemColumn)
                {
                    return new CheckeablePlasticTableCell<IPlasticTreeNode>(
                        true,
                        GetColumnText(columnName, node),
                        GetNodeIcon(node),
                        PlasticTableCell.StyleType.Normal,
                        PlasticTableCell.ColorType.Regular,
                        onCheckBoxClickedDelegate);
                }

                return new PlasticTableCell(
                    GetColumnText(columnName, node),
                    null,
                    isItemColumn ?
                        PlasticTableCell.StyleType.Normal :
                        PlasticTableCell.StyleType.Lighter,
                    PlasticTableCell.ColorType.Regular);
            };
        }

        internal static bool AreEqual(IPlasticTreeNode x, IPlasticTreeNode y)
        {
            if (x == null || y == null)
                return x == y;

            return GetKey.GetNodeKey(x) == GetKey.GetNodeKey(y);
        }

        static string GetColumnText(string columnName, IPlasticTreeNode node)
        {
            if (node is PendingChangeInfo)
                return ((PendingChangeInfo)node).GetColumnText(columnName);

            if (columnName != "ItemColumn")
                return string.Empty;

            return ((PendingChangeCategory)node).GetHeaderText();
        }

        static IImage GetNodeIcon(IPlasticTreeNode node)
        {
            if (node is PendingChangeCategory)
                return GetCategoryIcon((PendingChangeCategory)node);

            return GetIcon((PendingChangeInfo)node);
        }

        static IImage GetCategoryIcon(PendingChangeCategory category)
        {
            switch (category.Type)
            {
                case PendingChangeCategoryType.Added:
                    return /*PlasticImages.GetImage(PlasticImages.Name.IconAdded)*/null;
                case PendingChangeCategoryType.Changed:
                    return /*PlasticImages.GetImage(PlasticImages.Name.IconChanged)*/null;
                case PendingChangeCategoryType.Deleted:
                    return /*PlasticImages.GetImage(PlasticImages.Name.IconDeleted)*/null;
                case PendingChangeCategoryType.Moved:
                    return /*PlasticImages.GetImage(PlasticImages.Name.IconMoved)*/ null;
                default:
                    return null;
            }
        }

        static IImage GetIcon(PendingChangeInfo node)
        {
                    return null;
        }

        static class GetKey
        {
            internal static string GetNodeKey(IPlasticTreeNode node)
            {
                if (node is PendingChangeCategory)
                    return GetCategoryKey((PendingChangeCategory)node);

                return GetChangeKey((PendingChangeInfo)node);
            }

            static string GetCategoryKey(PendingChangeCategory category)
            {
                return category.Type.ToString();
            }

            static string GetChangeKey(PendingChangeInfo node)
            {
                return string.Format("{0}:{1}",
                    node.GetItemString(),
                    GetCategoryKey((PendingChangeCategory)node.GetParent()));
            }
        }

        const int ITEM_COLUMN_WIDTH = 460;
        const int STATUS_COLUMN_WIDTH = 183;
        const int SIZE_COLUMN_WIDTH = 65;
        const int DATE_MODIFIED_COLUMN_WIDTH = 215;
        const int EXTENSION_COLUMN_WIDTH = 74;
        const int TYPE_COLUMN_WIDTH = 48;
        const int SIMILARITY_COLUMN_WIDTH = 75;
        const int REPOSITORY_COLUMN_WIDTH = 155;
    }
}