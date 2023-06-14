using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Primitives
{
    public class TreeDataGridRowsPresenterTests_VariableHeight
    {
        [AvaloniaTheory(Timeout = 10000)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(50)]
        public void Scroll_Down_To_Bottom(double step)
        {
            var (target, scroll, _) = CreateTarget();

            Layout(target);

            var index = GetFirstRowIndex(target);
            Assert.Equal(0, index);

            while (scroll.Offset.Y < scroll.Extent.Height - scroll.Viewport.Height)
            {
                scroll.Offset = new Vector(0, scroll.Offset.Y + step);
                System.Diagnostics.Debug.WriteLine(scroll.Offset.Y);
                Layout(target);

                var newIndex = GetFirstRowIndex(target);
                Assert.True(newIndex >= index, $"{newIndex} > {index} failed");
                index = newIndex;
            }
        }

        [AvaloniaFact]
        public void Scroll_To_Bottom()
        {
            var (target, scroll, items) = CreateTarget();

            scroll.GetObservable(ScrollViewer.OffsetProperty).Subscribe(x => { });

            Layout(target);

            var index = GetFirstRowIndex(target);
            Assert.Equal(0, index);

            scroll.Offset = new Vector(0, scroll.Extent.Height - scroll.Viewport.Height);
            Layout(target);

            var lastIndex = GetLastRowIndex(target);
            Assert.Equal(items.Count - 1, lastIndex);
        }

        private static int GetFirstRowIndex(TreeDataGridRowsPresenter target)
        {
            return target!.GetVisualChildren()
                .Cast<TreeDataGridRow>()
                .Where(x => x.IsVisible)
                .Select(x => x.RowIndex)
                .OrderBy(x => x)
                .First();
        }

        private static int GetLastRowIndex(TreeDataGridRowsPresenter target)
        {
            return target!.GetVisualChildren()
                .Cast<TreeDataGridRow>()
                .Where(x => x.IsVisible)
                .Select(x => x.RowIndex)
                .OrderByDescending(x => x)
                .First();
        }

        private static (TreeDataGridRowsPresenter, ScrollViewer, AvaloniaList<Model>) CreateTarget(
            IColumns? columns = null, 
            List<IStyle>? additionalStyles = null,
            int itemCount = 100,
            Size? rootSize = null)
        {
            var rnd = new Random(0);
            var items = new AvaloniaList<Model>(Enumerable.Range(0, itemCount).Select(x =>
                new Model
                {
                    Id = x,
                    Height = rnd.Next(90) + 10,
                }));

            var itemsView = new TreeDataGridItemsSourceView<Model>(items);
            var rows = new AnonymousSortableRows<Model>(itemsView, null);

            var target = new TreeDataGridRowsPresenter
            {
                ElementFactory = new TreeDataGridElementFactory(),
                Items = rows,
                Columns = columns,
            };

            var scrollViewer = new ScrollViewer
            {
                Template = TestTemplates.ScrollViewerTemplate(),
                Content = target,
            };

            var root = new TestWindow(scrollViewer, rootSize ?? new Size(100, 1000))
            {
                Styles =
                {
                    new Style(x => x.OfType<TreeDataGridRow>())
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridRow.HeightProperty, new Binding(nameof(Model.Height))),
                        }
                    }
                },
                Height = 1000,
            };

            if (additionalStyles != null)
            {
                foreach (var item in additionalStyles)
                {
                    root.Styles.Add(item);
                }
            }

            root.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            return (target, scrollViewer, items);
        }

        private static void Layout(TreeDataGridRowsPresenter target)
        {
            target.UpdateLayout();
        }

        private class Model
        {
            public int Id { get; set; }
            public double Height { get; set; }
        }
    }
}
