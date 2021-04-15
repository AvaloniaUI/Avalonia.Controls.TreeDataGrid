using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.TreeDataGridTests
{
    internal static class TestTemplates
    {
        public static IControlTemplate ScrollViewerTemplate()
        {
            return new FuncControlTemplate<ScrollViewer>((x, ns) =>
                new ScrollContentPresenter
                {
                    Name = "PART_Presenter",
                    [~ContentPresenter.ContentProperty] = x[~ContentControl.ContentProperty],
                    [~~ScrollContentPresenter.ExtentProperty] = x[~~Avalonia.Controls.ScrollViewer.ExtentProperty],
                    [~~ScrollContentPresenter.OffsetProperty] = x[~~Avalonia.Controls.ScrollViewer.OffsetProperty],
                    [~~ScrollContentPresenter.ViewportProperty] = x[~~Avalonia.Controls.ScrollViewer.ViewportProperty],
                    [~ScrollContentPresenter.CanHorizontallyScrollProperty] = x[~Avalonia.Controls.ScrollViewer.CanHorizontallyScrollProperty],
                    [~ScrollContentPresenter.CanVerticallyScrollProperty] = x[~Avalonia.Controls.ScrollViewer.CanVerticallyScrollProperty],
                }.RegisterInNameScope(ns));
        }

        public static IControlTemplate TreeDataGridTemplate()
        {
            return new FuncControlTemplate<TreeDataGrid>((x, ns) =>
                new DockPanel
                {
                    Children =
                    {
                        new TreeDataGridColumnHeadersPresenter
                        {
                            Name = "PART_ColumnHeadersPresenter",
                            [!TreeDataGridColumnHeadersPresenter.ElementFactoryProperty] = x[!TreeDataGrid.ElementFactoryProperty],
                            [!TreeDataGridColumnHeadersPresenter.ItemsProperty] = x[!TreeDataGrid.ColumnsProperty],
                        }.RegisterInNameScope(ns),
                        new ScrollViewer
                        {
                            Name = "PART_ScrollViewer",
                            Template = ScrollViewerTemplate(),
                            Content = new TreeDataGridRowsPresenter
                            {
                                Name = "PART_RowsPresenter",
                                [!TreeDataGridRowsPresenter.ColumnsProperty] = x[!TreeDataGrid.ColumnsProperty],
                                [!TreeDataGridRowsPresenter.ElementFactoryProperty] = x[!TreeDataGrid.ElementFactoryProperty],
                                [!TreeDataGridRowsPresenter.ItemsProperty] = x[!TreeDataGrid.RowsProperty],
                            }.RegisterInNameScope(ns),
                        }.RegisterInNameScope(ns)
                    }
                });
        }

        public static IControlTemplate TreeDataGridRowTemplate()
        {
            return new FuncControlTemplate<TreeDataGridRow>((x, ns) =>
                new DockPanel
                {
                    Children =
                    {
                        new TreeDataGridCellsPresenter
                        {
                            Name = "PART_CellsPresenter",
                            [!TreeDataGridCellsPresenter.ElementFactoryProperty] = x[!TreeDataGridRow.ElementFactoryProperty],
                            [!TreeDataGridCellsPresenter.ItemsProperty] = x[!TreeDataGridRow.ColumnsProperty],
                            [!TreeDataGridCellsPresenter.RowsProperty] = x[!TreeDataGridRow.RowsProperty],
                        }.RegisterInNameScope(ns),
                    }
                });
        }
    }
}
