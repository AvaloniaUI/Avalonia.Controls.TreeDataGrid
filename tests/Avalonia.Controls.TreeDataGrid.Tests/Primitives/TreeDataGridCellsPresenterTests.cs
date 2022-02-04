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
                Assert.Equal(i < 10 ? 10 : double.NaN, column.ActualWidth);
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

        [Fact]
        public void DesiredSize_Takes_Min_Star_Column_Width_Into_Account()
        {
            using var app = App();

            var minWidth = new ColumnOptions<Model>
            {
                MinWidth = new GridLength(100),
            };

            var columns = new ColumnList<Model>
            {
                new LayoutTestColumn<Model>("Col0", GridLength.Star, minWidth),
                new LayoutTestColumn<Model>("Col1", GridLength.Star, minWidth),
            };

            var (target, scroll) = CreateTarget(columns);

            Assert.Equal(200, target.DesiredSize.Width);
        }

        [Fact]
        public void Star_Cells_Are_Measured_With_Final_Column_Width()
        {
            // Issue #70
            using var app = App();

            var columns = new ColumnList<Model>
            {
                new LayoutTestColumn<Model>("Col0", GridLength.Star),
                new LayoutTestColumn<Model>("Col1", GridLength.Star),
            };

            var (target, scroll) = CreateTarget(columns);

            foreach (LayoutTestCellControl? cell in target.RealizedElements)
            {
                Assert.Equal(
                    new[]
                    {
                        Size.Infinity,
                        new Size(50, 10),
                    },
                    cell!.MeasureConstraints);
            }
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

        private static (TreeDataGridCellsPresenter, ScrollViewer) CreateTarget(
            ColumnList<Model>? columns = null)
        {
            if (columns is null)
            {
                columns = new ColumnList<Model>();

                for (var i = 0; i < 100; ++i)
                    columns.Add(new LayoutTestColumn<Model>("Column " + i));
            }

            var items = new Model[1];
            var rows = new AnonymousSortableRows<Model>(new ItemsSourceViewFix<Model>(items), null);

            var target = new TreeDataGridCellsPresenter
            {
                ElementFactory = new TestElementFactory(),
                Items = columns,
                Rows = rows,
            };

            target.Realize(0);

            var scrollViewer = new ScrollViewer
            {
                Template = TestTemplates.ScrollViewerTemplate(),
                Content = target,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            var root = new TestRoot(scrollViewer);

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
    }
}
