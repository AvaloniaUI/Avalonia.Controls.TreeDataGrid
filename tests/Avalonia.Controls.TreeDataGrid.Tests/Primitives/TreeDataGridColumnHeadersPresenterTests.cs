using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Primitives
{
    public class TreeDataGridColumnHeadersPresenterTests
    {
        [AvaloniaFact(Timeout = 10000)]
        public void Creates_Initial_Columns()
        {
            var (target, _) = CreateTarget();

            AssertColumnIndexes(target, 0, 10);
            AssertRecyclable(target, 0);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Updates_Auto_Column_ActualWidth()
        {
            // We're testing Auto columns so make cells have a width of 10.
            var root = new TestWindow
            {
                Styles =
                {
                    new Style(x => x.OfType<TreeDataGridColumnHeader>())
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridRow.WidthProperty, 10.0),
                        }
                    }
                }
            };

            var (target, _) = CreateTarget(root: root, width: GridLength.Auto);

            for (var i = 0; i < target.Items!.Count; ++i)
            {
                var column = target.Items[i];
                Assert.Equal(i < 10 ? 10 : double.NaN, column.ActualWidth);
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Scrolls_Right_One_Row()
        {
            var (target, scroll) = CreateTarget();

            scroll.Offset = new Vector(10, 00);
            Layout(target);

            AssertColumnIndexes(target, 1, 10);
            AssertRecyclable(target, 0);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Scrolls_Right_More_Than_A_Page()
        {
            var (target, scroll) = CreateTarget();

            scroll.Offset = new Vector(200, 0);
            Layout(target);

            AssertColumnIndexes(target, 20, 10);
            AssertRecyclable(target, 0);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Scrolls_Left_More_Than_A_Page()
        {
            var (target, scroll) = CreateTarget();

            scroll!.Offset = new Vector(200, 0);
            Layout(target);

            scroll!.Offset = new Vector(0, 0);
            Layout(target);

            AssertColumnIndexes(target, 0, 10);
            AssertRecyclable(target, 0);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Updates_Column_Width_When_Width_Changes()
        {
            var (target, _) = CreateTarget();

            foreach (var child in target.GetVisualChildren())
                Assert.Equal(10, child.Bounds.Width);

            ((IColumns)target.Items!).SetColumnWidth(1, new GridLength(12, GridUnitType.Pixel));
            Layout(target);

            foreach (TreeDataGridColumnHeader column in target.GetVisualChildren())
            {
                Assert.Equal(column.ColumnIndex == 1 ? 12 : 10, column.Bounds.Width);
            }
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Nth_Child_Handles_Deletion_And_Addition_Correctly()
        {
            var (target, scroll) = CreateTarget(additionalStyles:
                new List<IStyle>
                {
                    new Style(x => x.OfType<TreeDataGridColumnHeadersPresenter>().Descendant().OfType<TreeDataGridColumnHeader>().NthChild(2,0))
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridRow.BackgroundProperty,new SolidColorBrush(Colors.Red)),
                        }
                    }
                });

            Layout(target);

            int CountEvenRedRows(TreeDataGridColumnHeadersPresenter presenter)
            {
                return target.GetVisualChildren().Cast<TreeDataGridColumnHeader>().Select(x => x.Background)
                    .Where(x => x is SolidColorBrush brush && brush.Color == Colors.Red).Count();
            }

            Assert.Equal(5, CountEvenRedRows(target));
        }

        private static void AssertColumnIndexes(
            TreeDataGridColumnHeadersPresenter? target,
            int firstColumnIndex,
            int columnCount)
        {
            Assert.NotNull(target);

            var rowIndexes = target!.GetVisualChildren()
                .Cast<TreeDataGridColumnHeader>()
                .Where(x => x.IsVisible)
                .Select(x => x.ColumnIndex)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(
                Enumerable.Range(firstColumnIndex, columnCount),
                rowIndexes);
        }

        private static void AssertRecyclable(TreeDataGridColumnHeadersPresenter? target, int count)
        {
            Assert.NotNull(target);

            var recyclableRows = target!.GetVisualChildren()
                .Cast<TreeDataGridColumnHeader>()
                .Where(x => !x.IsVisible)
                .ToList();
            Assert.Equal(count, recyclableRows.Count);
        }

        private static (TreeDataGridColumnHeadersPresenter, ScrollViewer) CreateTarget(
            TestWindow? root = null,
            GridLength? width = null,
            int columnCount = 100,
            List<IStyle>? additionalStyles = null)
        {
            var columns = new ColumnList<string>();

            width ??= new GridLength(10);

            for (var i = 0; i < columnCount; ++i)
                columns.Add(new TestColumn("Column " + i, width.Value));

            var target = new TreeDataGridColumnHeadersPresenter
            {
                ElementFactory = new TreeDataGridElementFactory(),
                Items = columns,
            };

            var scrollViewer = new ScrollViewer
            {
                Template = TestTemplates.ScrollViewerTemplate(),
                Content = target,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            root ??= new TestWindow();
            root.Content = scrollViewer;

            if (additionalStyles != null)
            {
                foreach (var item in additionalStyles)
                {
                    root.Styles.Add(item);
                }
            }

            root.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            return (target, scrollViewer);
        }

        private static void Layout(TreeDataGridColumnHeadersPresenter target)
        {
            target.UpdateLayout();
        }
        
        private class TestColumn : ColumnBase<string>
        {
            public TestColumn(string header, GridLength width)
                : base(header, width, NoMinWidth())
            {
            }

            public override ICell CreateCell(IRow<string> row)
            {
                throw new NotImplementedException();
            }

            public override Comparison<string?>? GetComparison(ListSortDirection direction)
            {
                throw new NotImplementedException();
            }

            private static ColumnOptions<string> NoMinWidth()
            {
                return new ColumnOptions<string>
                {
                    MinWidth = new GridLength(0, GridUnitType.Pixel)
                };
            }
        }
    }
}
