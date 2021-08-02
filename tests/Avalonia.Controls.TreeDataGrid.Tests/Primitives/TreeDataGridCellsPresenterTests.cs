using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Primitives
{
    public class TreeDataGridCellsPresenterTests
    {
        [Fact]
        public void Creates_Initial_Cells()
        {
            using var app = App();

            var (target, _) = CreateTarget();

            AssertColumnIndexes(target, 0, 10);
            AssertRecyclable(target, 0);
        }

        [Fact]
        public void Updates_Column_ActualWidth()
        {
            using var app = App();

            var (target, _) = CreateTarget();

            for (var i = 0; i < target.Items!.Count; ++i)
            {
                var column = target.Items[i];
                Assert.Equal(i < 10 ? (double?)10 : null, column.ActualWidth);
            }
        }

        [Fact]
        public void Scrolls_Right_One_Cell()
        {
            using var app = App();

            var (target, scroll) = CreateTarget();
            
            scroll.Offset = new Vector(10, 0);
            Layout(target);

            AssertColumnIndexes(target, 1, 10);
            AssertRecyclable(target, 0);
        }

        [Fact]
        public void Scrolls_Right_More_Than_A_Page()
        {
            using var app = App();

            var (target, scroll) = CreateTarget();

            scroll.Offset = new Vector(200, 0);
            Layout(target);

            AssertColumnIndexes(target, 20, 10);
            AssertRecyclable(target, 0);
        }

        [Fact]
        public void Scrolls_Left_More_Than_A_Page()
        {
            using var app = App();

            var (target, scroll) = CreateTarget();

            scroll.Offset = new Vector(200, 0);
            Layout(target);

            scroll.Offset = new Vector(0, 0);
            Layout(target);

            AssertColumnIndexes(target, 0, 10);
            AssertRecyclable(target, 0);
        }

        private static void AssertColumnIndexes(
            TreeDataGridCellsPresenter? target,
            int firstColumnIndex,
            int columnCount)
        {
            Assert.NotNull(target);

            var rowIndexes = target.GetLogicalChildren()
                .Cast<TreeDataGridCell>()
                .Where(x => x.IsVisible)
                .Select(x => x.ColumnIndex)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(
                Enumerable.Range(firstColumnIndex, columnCount),
                rowIndexes);
        }

        private static void AssertRecyclable(TreeDataGridCellsPresenter? target, int count)
        {
            Assert.NotNull(target);

            var recyclableCells = target.GetLogicalChildren()
                .Cast<TreeDataGridCell>()
                .Where(x => !x.IsVisible)
                .ToList();
            Assert.Equal(count, recyclableCells.Count);
        }

        private static (TreeDataGridCellsPresenter, ScrollViewer) CreateTarget()
        {
            var columns = new ColumnList<Model>();

            for (var i = 0; i < 100; ++i)
                columns.Add(new TestTextColumn("Column " + i));

            var items = new Model[1];
            var rows = new AnonymousSortableRows<Model>(new ItemsSourceViewFix<Model>(items), null);

            var target = new TreeDataGridCellsPresenter
            {
                ElementFactory = new TreeDataGridElementFactory(),
                Items = columns,
                Rows = rows,
            };

            target.Realize(0);

            var scrollViewer = new ScrollViewer
            {
                Template = TestTemplates.ScrollViewerTemplate(),
                Content = target,
            };

            var root = new TestRoot
            {
                Styles =
                {
                    new Style(x => x.Is<TreeDataGridCell>())
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridCell.WidthProperty, 10.0),
                        }
                    }
                },
                Child = scrollViewer,
            };

            root.LayoutManager.ExecuteInitialLayoutPass();
            return (target, scrollViewer);
        }

        private static void Layout(TreeDataGridCellsPresenter target)
        {
            var root = (ILayoutRoot)target.GetVisualRoot();
            root.LayoutManager.ExecuteLayoutPass();
        }

        private static IDisposable App()
        {
            var scope = AvaloniaLocator.EnterScope();
            AvaloniaLocator.CurrentMutable.Bind<IStyler>().ToConstant(new Styler());
            return scope;
        }

        private class Model : NotifyingBase
        {
            public int Id { get; set; }
            public string? Title { get; set; }
        }

        private class TestTextColumn : ColumnBase<Model>
        {
            public TestTextColumn(string header) : base(header, null, null)
            {
            }

            public override ICell CreateCell(IRow<Model> row)
            {
                return new TextCell<string>($"{Header} Row {row.ModelIndex}");
            }

            public override Comparison<Model>? GetComparison(ListSortDirection direction)
            {
                throw new NotImplementedException();
            }
        }
    }
}
