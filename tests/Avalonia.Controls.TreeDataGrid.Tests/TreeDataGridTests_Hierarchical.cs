using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests
{
    public class TreeDataGridTests_Hierarchical
    {
        [AvaloniaFact(Timeout = 10000)]
        public void Should_Display_Initial_Row_And_Cells()
        {
            var (target, _) = CreateTarget();

            Assert.NotNull(target.RowsPresenter);

            var rows = target.RowsPresenter!
                .GetVisualChildren()
                .Cast<TreeDataGridRow>()
                .ToList();
            
            Assert.Equal(2, rows.Count);

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
        public void Should_Display_Expanded_Root_Node()
        {
            var (target, source) = CreateTarget();

            Assert.NotNull(target.RowsPresenter);
            Assert.Equal(2, target.RowsPresenter!.RealizedElements.Count);
            Assert.Equal(2, target.RowsPresenter!.GetVisualChildren().Count());

            source.Expand(new IndexPath(0));

            Assert.Equal(102, source.Rows.Count);
            Assert.Equal(102, target.RowsPresenter!.RealizedElements.Count);
            Assert.Equal(2, target.RowsPresenter!.GetVisualChildren().Count());

            Layout(target);

            Assert.Equal(10, target.RowsPresenter!.RealizedElements.Count);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Display_Added_Root_Node()
        {
            var (target, source) = CreateTarget();
            var items = (IList<Model>)source.Items;

            Layout(target);
            items.Add(new Model { Id = -1, Title = "Added" });
            Layout(target);

            Assert.Equal(3, target.RowsPresenter!.RealizedElements.Count);
            Assert.Equal(3, target.RowsPresenter!.GetVisualChildren().Count());
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Display_Added_Child_Node()
        {
            var (target, source) = CreateTarget();
            var items = (IList<Model>)source.Items;
            var children = items[1].Children = new AvaloniaList<Model>
            {
                new Model { Id = -1, Title = "First" }
            };

            Layout(target);
            source.Expand(new IndexPath(1));
            Layout(target);
            children.Add(new Model { Id = -2, Title = "Second" });
            Layout(target);

            Assert.Equal(4, target.RowsPresenter!.RealizedElements.Count);
            Assert.Equal(4, target.RowsPresenter!.GetVisualChildren().Count());
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Subscribe_To_Models_For_Initial_Rows()
        {
            var (target, source) = CreateTarget();
            var items = (IList<Model>)source.Items;

            for (var i = 0; i < items.Count; ++i)
            {
                Assert.Equal(2, items[i].PropertyChangedSubscriberCount());
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Subscribe_To_Models_For_Expanded_Rows()
        {
            var (target, source) = CreateTarget();
            var items = (IList<Model>)source.Items;

            source.Expand(new IndexPath(0));
            Layout(target);

            Assert.Equal(2, items[0].PropertyChangedSubscriberCount());
            Assert.Equal(0, items[1].PropertyChangedSubscriberCount());

            var children = items[0].Children!;
            for (var i = 0; i < children.Count; ++i)
            {
                var expected = i < 9 ? 2 : 0;
                Assert.Equal(expected, children[i].PropertyChangedSubscriberCount());
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Subscribe_To_Correct_Models_After_Scrolling_Down_One_Row()
        {
            var (target, source) = CreateTarget();
            var items = (IList<Model>)source.Items;

            source.Expand(new IndexPath(0));
            Layout(target);
            target.Scroll!.Offset = new Vector(0, 10);
            Layout(target);

            Assert.Equal(0, items[0].PropertyChangedSubscriberCount());
            Assert.Equal(0, items[1].PropertyChangedSubscriberCount());

            var children = items[0].Children!;
            for (var i = 0; i < children.Count; ++i)
            {
                var expected = i < 10 ? 2 : 0;
                Assert.Equal(expected, children[i].PropertyChangedSubscriberCount());
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Scrolling_Should_Not_Rebuild_Templates_In_Expander_Columns()
        {
            var instantiations = 0;

            Control Template(Model model, INameScope ns)
            {
                ++instantiations;
                return new Border();
            }

            var columns = new IColumn<Model>[]
            {
                new HierarchicalExpanderColumn<Model>(
                    new TemplateColumn<Model>("ID", new FuncDataTemplate<Model>(Template, true)),
                    x => x.Children,
                    x => true),
                new TextColumn<Model, string?>("Title", x => x.Title),
            };

            // Create the TreeDataGrid but don't do an initial layout.
            var (target, source) = CreateTarget(columns: columns, runLayout: false);
            var items = (IList<Model>)source.Items;

            // Expand the first root and do the initial layout now.
            instantiations = 0;
            source.Expand(new IndexPath(0));
            InitialLayout(target);
            Assert.Equal(9, instantiations);

            // Scroll down a row.
            target.Scroll!.Offset = new Vector(0, 10);
            Layout(target);

            // Template should have been recycled and not rebuilt.
            Assert.Equal(9, instantiations);
            Assert.Equal(10, target.RowsPresenter!.RealizedElements.Count);

            for (var i = 0; i < 10; ++i)
            {
                var row = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[i]!;
                var cell = (TreeDataGridExpanderCell)row.CellsPresenter!.RealizedElements[0]!;
                var inner = cell.FindDescendantOfType<TreeDataGridTemplateCell>()!;
                var innerModel = (TemplateCell)inner.DataContext!;
                var rowModel = source.Rows[i + 1].Model;

                Assert.Equal(rowModel, row.DataContext);
                Assert.Equal(rowModel, cell.DataContext);
                Assert.Equal(rowModel, innerModel.Value);
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Unsubscribe_From_Models_When_Detached_From_Logical_Tree()
        {
            var (target, source) = CreateTarget();
            var items = (IList<Model>)source.Items;

            ((Window)target.Parent!).Content = null;

            for (var i = 0; i < items.Count; ++i)
            {
                Assert.Equal(0, items[i].PropertyChangedSubscriberCount());
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Should_Hide_Expander_When_Node_With_No_Children_Expanded()
        {
            var (target, source) = CreateTarget();
            var cell = target.TryGetCell(0, 1);
            var expander = Assert.IsType<TreeDataGridExpanderCell>(cell);

            Assert.False(expander.IsExpanded);
            Assert.True(expander.ShowExpander);

            expander.IsExpanded = true;

            Assert.False(expander.IsExpanded);
            Assert.False(expander.ShowExpander);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Can_Reassign_Items_When_Displaying_Child_Items_Followed_By_Root_Items()
        {
            var (target, source) = CreateTarget();
            var cell = target.TryGetCell(0, 0);
            var expander = Assert.IsType<TreeDataGridExpanderCell>(cell);

            // Add a a few more root items.
            ((AvaloniaList<Model>)source.Items).AddRange(CreateModels("Root ", 5, firstIndex: 2));

            // Expand the first root item and scroll down such that we're displaying some children
            // of the first root item together with subsequent root items.
            source.Expand(new IndexPath(0));
            Layout(target);
            target.Scroll!.Offset = new Vector(0, 1970);
            Layout(target);

            var firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.First()!;
            var lastRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.Last()!;
            var firstRowModel = (IRow<Model>)source.Rows[firstRow.RowIndex];
            var lastRowModel = (IRow<Model>)source.Rows[lastRow.RowIndex];

            Assert.Equal("Item 0-96", firstRowModel.Model.Title);
            Assert.Equal("Root 6", lastRowModel.Model.Title);

            // Replace the items with a single item.
            source.Items = new AvaloniaList<Model>
            {
                new Model
                {
                    Id = 0,
                    Title = "Root 0",
                },
            };

            Layout(target);

            firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[0]!;
            Assert.Equal(0, firstRow.RowIndex);
            Assert.Equal(new Vector(0, 0), target.Scroll!.Offset);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Can_Reassign_Items_When_Displaying_Grandchild_Items_Followed_By_Root_Items()
        {
            var (target, source) = CreateTarget();
            var cell = target.TryGetCell(0, 0);
            var expander = Assert.IsType<TreeDataGridExpanderCell>(cell);

            // Add a a few more root items.
            ((AvaloniaList<Model>)source.Items).AddRange(CreateModels("Root ", 5, firstIndex: 2));

            // Add some grandchildren.
            ((AvaloniaList<Model>)source.Items)[0].Children!.AddRange(CreateModels("Item 0-0-", 100));

            // Expand the first child item and scroll down such that we're displaying some children
            // of the first root item together with subsequent root items.
            source.Expand(new IndexPath(0, 0));
            Layout(target);
            target.Scroll!.Offset = new Vector(0, 1970);
            Layout(target);

            var firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.First()!;
            var lastRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.Last()!;
            var firstRowModel = (IRow<Model>)source.Rows[firstRow.RowIndex];
            var lastRowModel = (IRow<Model>)source.Rows[lastRow.RowIndex];

            Assert.Equal("Item 0-0-96", firstRowModel.Model.Title);
            Assert.Equal("Root 6", lastRowModel.Model.Title);

            // Replace the items with a single item.
            source.Items = new AvaloniaList<Model>
            {
                new Model
                {
                    Id = 0,
                    Title = "Root 0",
                },
            };

            Layout(target);

            firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[0]!;
            Assert.Equal(0, firstRow.RowIndex);
            Assert.Equal(new Vector(0, 0), target.Scroll!.Offset);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Can_Reset_Items_When_Displaying_Child_Items_Followed_By_Root_Items()
        {
            var (target, source) = CreateTarget();
            var cell = target.TryGetCell(0, 0);
            var expander = Assert.IsType<TreeDataGridExpanderCell>(cell);

            // Add a a few more root items.
            ((AvaloniaList<Model>)source.Items).AddRange(CreateModels("Root ", 5, firstIndex: 2));

            // Expand the first root item and scroll down such that we're displaying some children
            // of the first root item together with subsequent root items.
            source.Expand(new IndexPath(0));
            Layout(target);
            target.Scroll!.Offset = new Vector(0, 1970);
            Layout(target);

            var firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.First()!;
            var lastRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.Last()!;
            var firstRowModel = (IRow<Model>)source.Rows[firstRow.RowIndex];
            var lastRowModel = (IRow<Model>)source.Rows[lastRow.RowIndex];

            Assert.Equal("Item 0-96", firstRowModel.Model.Title);
            Assert.Equal("Root 6", lastRowModel.Model.Title);

            // Clear the items.
            ((IList)source.Items).Clear();

            Layout(target);

            Assert.Empty(target.RowsPresenter!.RealizedElements);
            Assert.Equal(new Vector(0, 0), target.Scroll!.Offset);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Can_Reset_Child_Items_When_Displaying_Grandchild_Items_Followed_By_Root_Items()
        {
            var (target, source) = CreateTarget();
            var cell = target.TryGetCell(0, 0);
            var expander = Assert.IsType<TreeDataGridExpanderCell>(cell);

            // Add a a few more root items.
            ((AvaloniaList<Model>)source.Items).AddRange(CreateModels("Root ", 5, firstIndex: 2));

            // Add some grandchildren.
            ((AvaloniaList<Model>)source.Items)[0].Children!.AddRange(CreateModels("Item 0-0-", 100));

            // Expand the first child item and scroll down such that we're displaying some children
            // of the first root item together with subsequent root items.
            source.Expand(new IndexPath(0, 0));
            Layout(target);
            target.Scroll!.Offset = new Vector(0, 1970);
            Layout(target);

            var firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.First()!;
            var lastRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.Last()!;
            var firstRowModel = (IRow<Model>)source.Rows[firstRow.RowIndex];
            var lastRowModel = (IRow<Model>)source.Rows[lastRow.RowIndex];

            Assert.Equal("Item 0-0-96", firstRowModel.Model.Title);
            Assert.Equal("Root 6", lastRowModel.Model.Title);

            // Clear the child items.
            ((AvaloniaList<Model>)source.Items)[0].Children!.Clear();

            Layout(target);

            firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[0]!;
            Assert.Equal(0, firstRow.RowIndex);
            Assert.Equal(new Vector(0, 0), target.Scroll!.Offset);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Can_Remove_Selected_Item()
        {
            var (target, source) = CreateTarget();

            source.Expand(new IndexPath(0, 0));
            Layout(target);
            target.RowSelection!.Select(new IndexPath(0, 3));

            Assert.Equal(new IndexPath(0, 3), target.RowSelection.SelectedIndex);

            ((AvaloniaList<Model>)source.Items)[0].Children!.RemoveAt(3);

            Assert.Equal(-1, target.RowSelection.SelectedIndex);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Can_Remove_Selected_Item_Sorted()
        {
            var (target, source) = CreateTarget();
            target.Source!.SortBy(target.Columns![0], ListSortDirection.Descending);

            source.Expand(new IndexPath(0, 0));
            Layout(target);
            target.RowSelection!.Select(new IndexPath(0, 3));

            Assert.Equal(new IndexPath(0, 3), target.RowSelection.SelectedIndex);

            ((AvaloniaList<Model>)source.Items)[0].Children!.RemoveAt(3);

            Assert.Equal(-1, target.RowSelection.SelectedIndex);
        }

        private static (TreeDataGrid, HierarchicalTreeDataGridSource<Model>) CreateTarget(
            IEnumerable<IColumn<Model>>? columns = null,
            bool runLayout = true)
        {
            var items = new AvaloniaList<Model>
            {
                new Model
                {
                    Id = 0,
                    Title = "Root 0",
                    Children = new AvaloniaList<Model>(CreateModels("Item 0-", 100))
                },
                new Model
                {
                    Id = 1,
                    Title = "Root 1",
                },
            };

            columns ??= new IColumn<Model>[]
            {
                new HierarchicalExpanderColumn<Model>(
                    new TextColumn<Model, int>("ID", x => x.Id),
                    x => x.Children,
                    x => true),
                new TextColumn<Model, string?>("Title", x => x.Title),
            };

            var source = new HierarchicalTreeDataGridSource<Model>(items);
            source.Columns.AddRange(columns);

            var target = new TreeDataGrid
            {
                Template = TestTemplates.TreeDataGridTemplate(),
                Source = source,
            };

            var root = new TestWindow(target)
            {
                Styles =
                {
                    TestTemplates.TreeDataGridExpanderCellStyle,
                    TestTemplates.TreeDataGridTemplateCellStyle,
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
                root.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            return (target, source);
        }

        private static void Layout(TreeDataGrid target)
        {
            target.UpdateLayout();
        }

        private static void InitialLayout(TreeDataGrid target)
        {
            target.UpdateLayout();
        }

        private static IEnumerable<Model> CreateModels(
            string titlePrefix,
            int count,
            int firstIndex = 0,
            int firstId = 100)
        {
            return Enumerable.Range(0, count).Select(x =>
                new Model
                {
                    Id = firstId + firstIndex + x,
                    Title = titlePrefix + (firstIndex + x),
                });
        }

        private class Model : NotifyingBase
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public AvaloniaList<Model>? Children { get; set; }
        }
    }
}
