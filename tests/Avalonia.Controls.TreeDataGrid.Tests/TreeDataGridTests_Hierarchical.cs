using System;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests
{
    public class TreeDataGridTests_Hierarchical
    {
        [Fact]
        public void Should_Display_Initial_Row_And_Cells()
        {
            using var app = App();

            var (target, _) = CreateTarget();

            Assert.NotNull(target.RowsPresenter);

            var rows = target.RowsPresenter
                .GetLogicalChildren()
                .Cast<TreeDataGridRow>()
                .ToList();
            
            Assert.Equal(2, rows.Count);

            foreach (var row in rows)
            {
                var cells = row.CellsPresenter
                    .GetLogicalChildren()
                    .Cast<TreeDataGridCell>()
                    .ToList();
                Assert.Equal(2, cells.Count);
            }
        }

        [Fact]
        public void Should_Display_Expanded_Root_Node()
        {
            using var app = App();

            var (target, _) = CreateTarget();
            var source = (HierarchicalTreeDataGridSource<Model>)target.Source!;

            Assert.NotNull(target.RowsPresenter);
            Assert.Equal(2, target.RowsPresenter!.RealizedElements.Count());
            Assert.Equal(2, target.RowsPresenter!.GetLogicalChildren().Count());

            source.Expand(new IndexPath(0));

            Assert.Equal(102, source.Rows.Count);
            Assert.Equal(102, target.RowsPresenter!.RealizedElements.Count());
            Assert.Equal(2, target.RowsPresenter!.GetLogicalChildren().Count());

            Layout(target);

            Assert.Equal(10, target.RowsPresenter!.RealizedElements.Count());
            Assert.Equal(10, target.RowsPresenter!.GetLogicalChildren().Count());
        }

        private static (TreeDataGrid, AvaloniaList<Model>) CreateTarget()
        {
            var items = new AvaloniaList<Model>
            {
                new Model
                {
                    Id = 0,
                    Title = "Root 0",
                    Children = new AvaloniaList<Model>(Enumerable.Range(0, 100).Select(x =>
                        new Model
                        {
                            Id = 100 + x,
                            Title = "Item 0-" + x,
                        }))
                },
                new Model
                {
                    Id = 1,
                    Title = "Root 1",
                    Children = new AvaloniaList<Model>(Enumerable.Range(0, 100).Select(x =>
                        new Model
                        {
                            Id = 100 + x,
                            Title = "Item 1-" + x,
                        }))
                },
            };

            var source = new HierarchicalTreeDataGridSource<Model>(items);
            source.Columns.Add(
                new HierarchicalExpanderColumn<Model>(
                    new TextColumn<Model, int>("ID", x => x.Id),
                    x => x.Children,
                    x => x.Children is object));
            source.Columns.Add(new TextColumn<Model, string?>("Title", x => x.Title));

            var target = new TreeDataGrid
            {
                Template = TestTemplates.TreeDataGridTemplate(),
                Source = source,
            };

            var root = new TestRoot
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
                },
                Child = target,
            };

            root.LayoutManager.ExecuteInitialLayoutPass();
            return (target, items);
        }

        private static void Layout(TreeDataGrid target)
        {
            var root = (ILayoutRoot)target.GetVisualRoot();
            root.LayoutManager.ExecuteLayoutPass();
        }

        private static IDisposable App()
        {
            var scope = AvaloniaLocator.EnterScope();
            AvaloniaLocator.CurrentMutable.Bind<IStyler>().ToLazy(() => new Styler());
            return scope;
        }

        private class Model : NotifyingBase
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public AvaloniaList<Model>? Children { get; set; }
        }
    }
}
