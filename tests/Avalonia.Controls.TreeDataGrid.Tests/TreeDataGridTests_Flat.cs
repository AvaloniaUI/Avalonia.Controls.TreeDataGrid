using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Embedding;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Xunit;
using Enumerable = System.Linq.Enumerable;

namespace Avalonia.Controls.TreeDataGridTests
{
    public class TreeDataGridTests_Flat
    {
        [AvaloniaFact(Timeout = 10000)]
        public void Should_Display_Initial_Rows_And_Cells()
        {
            var (target, _) = CreateTarget();

            Assert.NotNull(target.RowsPresenter);

            var rows = target.RowsPresenter!
                .GetVisualChildren()
                .Cast<TreeDataGridRow>()
                .ToList();

            Assert.Equal(10, rows.Count);

            foreach (var row in rows)
            {
                var cells = row.CellsPresenter!
                    .GetVisualChildren()
                    .Cast<TreeDataGridCell>()
                    .ToList();
                Assert.Equal(2, cells.Count);
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void MultiSelection_Should_Work_Correctly_With_Duplicates()
        {
            var items = new List<Model>
            {
                new Model(){ Id=0, Title="Item 0"},
                new Model(){ Id=1, Title="Item 1"},
                new Model(){ Id=2, Title="Item 2"}
            };
            items.Add(items[0]);
            items.Add(items[0]);
            items.Add(items[0]);

            var (target, aaa) = CreateTarget(items);

            target.RowSelection!.Select(3);
            target.RowSelection.Select(4);
            target.RowSelection.Select(5);

            AssertInteractionSelection(target, 3, 4, 5);

            target.Source!.SortBy(target.Columns![0], ListSortDirection.Ascending);

            AssertInteractionSelection(target, 1, 2, 3);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Selection_Should_Be_Preserved_After_Sorting()
        {
            var (target, aaa) = CreateTarget();

            target.RowSelection!.Select(0);
            target.RowSelection.Select(5);

            AssertInteractionSelection(target, 0, 5);
            
            target.Source!.SortBy(target.Columns![0], ListSortDirection.Descending);

            ///There are 100 items in the collection.
            ///Their IDs are in range 0..99 so when we order IDs column in Descending order the latest element of the collection would be with
            ///ID 0(index 99 in collection),first with ID 99
            AssertInteractionSelection(target, 94, 99);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Subscribe_To_Models_For_Initial_Rows()
        {
            var (target, items) = CreateTarget();

            for (var i = 0; i < items.Count; ++i)
            {
                var expected = i < 10 ? 2 : 0;
                Assert.Equal(expected, items[i].PropertyChangedSubscriberCount());
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Subscribe_To_Correct_Models_After_Scrolling_Down_One_Row()
        {
            var (target, items) = CreateTarget();

            target.Scroll!.Offset = new Vector(0, 10);
            Layout(target);

            for (var i = 0; i < items.Count; ++i)
            {
                var expected = i > 0 && i <= 10 ? 2 : 0;
                Assert.Equal(expected, items[i].PropertyChangedSubscriberCount());
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Subscribe_To_Correct_Models_After_Scrolling_Down_One_Page()
        {
            var (target, items) = CreateTarget();

            target.Scroll!.Offset = new Vector(0, 100);
            Layout(target);

            for (var i = 0; i < items.Count; ++i)
            {
                var expected = i >= 10 && i < 20 ? 2 : 0;
                Assert.Equal(expected, items[i].PropertyChangedSubscriberCount());
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Unsubscribe_From_Models_When_Detached_From_Logical_Tree()
        {
            var (target, items) = CreateTarget();

            ((Window)target.Parent!).Content = null;

            for (var i = 0; i < items.Count; ++i)
            {
                Assert.Equal(0, items[i].PropertyChangedSubscriberCount());
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Desired_Width_Should_Be_Total_Of_Fixed_Width_Columns()
        {
            var (target, items) = CreateTarget(
                columns: new IColumn<Model>[]
                {
                    new TextColumn<Model, int>("ID", x => x.Id, new GridLength(10, GridUnitType.Pixel), MinWidth(0)),
                    new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(14, GridUnitType.Pixel), MinWidth(0))
                }
            );

            Assert.Equal(24, target.DesiredSize.Width);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Size_Star_Columns()
        {
            var (target, items) = CreateTarget(
                columns: new IColumn<Model>[]
                {
                    new TextColumn<Model, int>("ID", x => x.Id, new GridLength(1, GridUnitType.Star), MinWidth(0)),
                    new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(3, GridUnitType.Star), MinWidth(0))
                }
            );

            var rows = target.RowsPresenter!
                .GetVisualChildren()
                .Cast<TreeDataGridRow>()
                .ToList();

            Assert.Equal(10, rows.Count);

            foreach (var row in rows)
            {
                var cells = row.CellsPresenter!
                    .GetVisualChildren()
                    .Cast<TreeDataGridCell>()
                    .ToList();
                Assert.Equal(2, cells.Count);
                Assert.Equal(25, cells[0].Bounds.Width);
                Assert.Equal(75, cells[1].Bounds.Width);
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Size_Star_Columns_With_Min_Width()
        {
            var (target, items) = CreateTarget(
                columns: new IColumn<Model>[]
                {
                    new TextColumn<Model, int>("ID", x => x.Id, new GridLength(1, GridUnitType.Star), MinWidth(50)),
                    new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(3, GridUnitType.Star))
                }
            );

            var rows = target.RowsPresenter!
                .GetVisualChildren()
                .Cast<TreeDataGridRow>()
                .ToList();

            Assert.Equal(10, rows.Count);

            foreach (var row in rows)
            {
                var cells = row.CellsPresenter!
                    .GetVisualChildren()
                    .Cast<TreeDataGridCell>()
                    .ToList();
                Assert.Equal(2, cells.Count);
                Assert.Equal(50, cells[0].Bounds.Width);
                Assert.Equal(50, cells[1].Bounds.Width);
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Size_Star_Columns_With_Max_Width()
        {
            var (target, items) = CreateTarget(
                columns: new IColumn<Model>[]
                {
                    new TextColumn<Model, int>("ID", x => x.Id, new GridLength(1, GridUnitType.Star)),
                    new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(1, GridUnitType.Star), MaxWidth(25))
                }
            );

            var rows = target.RowsPresenter!
                .GetVisualChildren()
                .Cast<TreeDataGridRow>()
                .ToList();

            Assert.Equal(10, rows.Count);

            foreach (var row in rows)
            {
                var cells = row?.CellsPresenter!
                    .GetVisualChildren()
                    .Cast<TreeDataGridCell>()
                    .ToList();
                Assert.Equal(2, cells!.Count);
                Assert.Equal(75, cells[0].Bounds.Width);
                Assert.Equal(25, cells[1].Bounds.Width);
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Raises_CellPrepared_Events_On_Initial_Layout()
        {
            var (target, items) = CreateTarget(runLayout: false);
            var raised = 0;

            target.CellPrepared += (s, e) =>
            {
                ++raised;
            };

            target.UpdateLayout();

            Assert.Equal(20, raised);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Raises_CellClearing_CellPrepared_Events_On_Scroll()
        {
            var (target, items) = CreateTarget();
            var clearingRaised = 0;
            var preparedRaised = 0;

            target.CellClearing += (s, e) =>
            {
                Assert.Equal(clearingRaised % 2, e.ColumnIndex);
                Assert.Equal(0, e.RowIndex);
                ++clearingRaised;
            };

            target.CellPrepared += (s, e) =>
            {
                Assert.Equal(preparedRaised % 2, e.ColumnIndex);
                Assert.Equal(10, e.RowIndex);
                ++preparedRaised;
            };

            target.Scroll!.Offset = new Vector(0, 10);
            Layout(target);

            Assert.Equal(2, clearingRaised);
            Assert.Equal(2, preparedRaised);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Does_Not_Realize_Columns_Outside_Viewport()
        {
            var (target, items) = CreateTarget(columns: new IColumn<Model>[]
            {
                new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(1, GridUnitType.Star)),
                new TextColumn<Model, string?>("Title1", x => x.Title, options: MinWidth(50)),
                new TextColumn<Model, string?>("Title2", x => x.Title, options: MinWidth(50)),
                new TextColumn<Model, string?>("Title3", x => x.Title, options: MinWidth(50)),
            });

            AssertColumnIndexes(target, 0, 3);

            var columns = (ColumnList<Model>)target.Columns!;
            Assert.Equal(30, columns[0].ActualWidth);
            Assert.Equal(50, columns[1].ActualWidth);
            Assert.Equal(50, columns[2].ActualWidth);
            Assert.True(double.IsNaN(columns[3].ActualWidth));
        }

        public class RemoveItems
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Can_Remove_Range_Within_Realized_Elements()
            {
                var (target, items) = CreateTarget();

                target.Scroll!.Offset = new Vector(0, 100);
                Layout(target);

                AssertRowIndexes(target, 10, 10);

                items.RemoveRange(12, 4);
                Layout(target);

                AssertRowIndexes(target, 10, 10);
                Assert.Equal(new Vector(0, 100), target.Scroll.Offset);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Remove_Range_Within_Realized_Elements_When_Scrolled_To_End()
            {
                var (target, items) = CreateTarget(itemCount: 20);

                target.Scroll!.Offset = new Vector(0, 100);
                Layout(target);

                AssertRowIndexes(target, 10, 10);

                items.RemoveRange(12, 4);
                Layout(target);

                AssertRowIndexes(target, 6, 10);
                Assert.Equal(new Vector(0, 60), target.Scroll.Offset);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Remove_Range_Of_All_Realized_Elements()
            {
                var (target, items) = CreateTarget();

                target.Scroll!.Offset = new Vector(0, 100);
                Layout(target);

                AssertRowIndexes(target, 10, 10);

                items.RemoveRange(10, 10);
                Layout(target);

                AssertRowIndexes(target, 10, 10);
                Assert.Equal(new Vector(0, 100), target.Scroll.Offset);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Remove_Range_Of_All_Realized_Elements_When_Scrolled_To_End()
            {
                var (target, items) = CreateTarget(itemCount: 20);

                target.Scroll!.Offset = new Vector(0, 100);
                Layout(target);

                AssertRowIndexes(target, 10, 10);

                items.RemoveRange(10, 10);
                Layout(target);

                AssertRowIndexes(target, 0, 10);
                Assert.Equal(new Vector(0, 0), target.Scroll.Offset);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Remove_Range_Spanning_Beginning_Of_Realized_Elements_When_Scrolled_To_End()
            {
                var (target, items) = CreateTarget(itemCount: 20);

                target.Scroll!.Offset = new Vector(0, 100);
                Layout(target);

                AssertRowIndexes(target, 10, 10);

                items.RemoveRange(5, 10);
                Layout(target);

                AssertRowIndexes(target, 0, 10);
                Assert.Equal(new Vector(0, 0), target.Scroll.Offset);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Remove_Range_Spanning_End_Of_Realized_Elements()
            {
                var (target, items) = CreateTarget();

                target.Scroll!.Offset = new Vector(0, 100);
                Layout(target);

                AssertRowIndexes(target, 10, 10);

                items.RemoveRange(15, 10);
                Layout(target);

                AssertRowIndexes(target, 10, 10);
                Assert.Equal(new Vector(0, 100), target.Scroll.Offset);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Remove_Selected_Item()
            {
                var (target, items) = CreateTarget();

                Layout(target);
                target.RowSelection!.Select(3);

                Assert.Equal(3, target.RowSelection.SelectedIndex);

                items.RemoveAt(3);

                Assert.Equal(-1, target.RowSelection.SelectedIndex);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Remove_Selected_Item_Sorted()
            {
                var (target, items) = CreateTarget();
                target.Source!.SortBy(target.Columns![0], ListSortDirection.Descending);

                Layout(target);
                target.RowSelection!.Select(3);

                Assert.Equal(3, target.RowSelection.SelectedIndex);

                items.RemoveAt(3);

                Assert.Equal(-1, target.RowSelection.SelectedIndex);
            }
        }

        private static void AssertRowIndexes(TreeDataGrid target, int firstRowIndex, int rowCount)
        {
            var presenter = target.RowsPresenter;

            Assert.NotNull(presenter);

            var rowIndexes = presenter?.GetVisualChildren()
                .Cast<TreeDataGridRow>()
                .Where(x => x.IsVisible)
                .Select(x => x.RowIndex)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(
                Enumerable.Range(firstRowIndex, rowCount),
                rowIndexes);

            rowIndexes = presenter!.RealizedElements
                .Cast<TreeDataGridRow>()
                .Where(x => x.IsVisible)
                .Select(x => x.RowIndex)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(
                Enumerable.Range(firstRowIndex, rowCount),
                rowIndexes);
        }

        private static void AssertColumnIndexes(TreeDataGrid target, int firstColumnIndex, int columnCount)
        {
            var presenter = target.ColumnHeadersPresenter;

            Assert.NotNull(presenter);

            var columnIndexes = presenter?.GetVisualChildren()
                .Cast<TreeDataGridColumnHeader>()
                .Where(x => x.IsVisible)
                .Select(x => x.ColumnIndex)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(
                Enumerable.Range(firstColumnIndex, columnCount),
                columnIndexes);

            columnIndexes = presenter!.RealizedElements
                .Cast<TreeDataGridColumnHeader>()
                .Where(x => x.IsVisible)
                .Select(x => x.ColumnIndex)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(
                Enumerable.Range(firstColumnIndex, columnCount),
                columnIndexes);
        }

        private static void AssertInteractionSelection(TreeDataGrid target, params int[] selected)
        {
            var selection = (ITreeDataGridSelectionInteraction)target.RowSelection!;

            for (var i = 0; i < target.Rows!.Count; ++i)
            {
                Assert.Equal(selected.Contains(i), selection.IsRowSelected(i));
            }
        }

        private static (TreeDataGrid, AvaloniaList<Model>) CreateTarget(IEnumerable<Model>? models = null,
            IEnumerable<IColumn<Model>>? columns = null,
            int itemCount = 100,
            bool runLayout = true)
        {
            AvaloniaList<Model>? items = null;
            if (models == null)
            {
                items = new AvaloniaList<Model>(Enumerable.Range(0, itemCount).Select(x =>
                    new Model
                    {
                        Id = x,
                        Title = "Item " + x,
                    }));
            }
            else
            {
                items = new AvaloniaList<Model>(models);
            }


            var source = new FlatTreeDataGridSource<Model>(items);
            source.RowSelection!.SingleSelect = false;

            if (columns is object)
            {
                foreach (var column in columns)
                    source.Columns.Add(column);
            }
            else
            {
                source.Columns.Add(new TextColumn<Model, int>("ID", x => x.Id));
                source.Columns.Add(new TextColumn<Model, string?>("Title", x => x.Title));
            }

            var target = new TreeDataGrid
            {
                Template = TestTemplates.TreeDataGridTemplate(),
                Source = source,
            };

            var root = new TestWindow(target)
            {
                Styles =
                {
                    new Style(x => x.Is<TreeDataGridRow>())
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridRow.TemplateProperty, TestTemplates.TreeDataGridRowTemplate()),
                        }
                    },
                    new Style(x => x.Is<TreeDataGridCell>())
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridCell.HeightProperty, 10.0),
                        }
                    }
                }
            };

            if (runLayout)
            {
                root.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
            }

            return (target, items);
        }

        private static void Layout(TreeDataGrid target)
        {
            target.UpdateLayout();
        }

        private static TextColumnOptions<Model> MinWidth(double min)
        {
            return new TextColumnOptions<Model>
            {
                MinWidth = new GridLength(min, GridUnitType.Pixel),
            };
        }

        private static TextColumnOptions<Model> MaxWidth(double max)
        {
            return new TextColumnOptions<Model>
            {
                MaxWidth = new GridLength(max, GridUnitType.Pixel),
            };
        }

        private class Model : NotifyingBase
        {
            public int Id { get; set; }
            public string? Title { get; set; }
        }
    }
}
