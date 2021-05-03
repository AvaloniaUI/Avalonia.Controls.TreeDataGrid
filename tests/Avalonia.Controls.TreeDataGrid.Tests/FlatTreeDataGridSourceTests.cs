using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Models;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Diagnostics;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests
{
    public class FlatTreeDataGridSourceTests
    {
#if false
        [Fact]
        public void Creates_Initial_Cells()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);
            AssertCells(target.Cells, data);
        }

        [Fact]
        public void Creates_Cells_For_Added_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data.Add(new Row { Id = 10, Caption = "New Row 10" });

            Assert.Equal(11, target.Rows.Count);
            Assert.Equal(2, raised);

            AssertCells(target.Cells, data);
        }

        [Fact]
        public void Removes_Cells_For_Removed_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data.RemoveAt(5);

            Assert.Equal(9, target.Rows.Count);
            Assert.Equal(1, raised);

            AssertCells(target.Cells, data);
        }

        [Fact]
        public void Updates_Cells_For_Replaced_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data[5] = new Row { Id = 10, Caption = "New Row 10" };

            Assert.Equal(10, target.Rows.Count);
            Assert.Equal(2, raised);

            var cell0 = Assert.IsType<TextCell<int>>(target.Cells[0, 5]);
            var cell1 = Assert.IsType<TextCell<string>>(target.Cells[1, 5]);

            AssertCells(target.Cells, data);
        }

        [Fact]
        public void Updates_Cells_For_Moved_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data.Move(5, 8);

            Assert.Equal(10, target.Rows.Count);
            Assert.Equal(3, raised);

            AssertCells(target.Cells, data);
        }

        [Fact]
        public void Updates_Cells_For_Clear()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data.Clear();

            Assert.Equal(0, target.Rows.Count);
            Assert.Equal(1, raised);

            AssertCells(target.Cells, data);
        }

        [Fact]
        public void Updates_Cells_On_Property_Change()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal("Row 0", target.Cells[1, 0].Value);

            data[0].Caption = "Modified";

            Assert.Equal("Modified", target.Cells[1, 0].Value);
        }

        [Fact]
        public void Can_Reassign_Items()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var rowsResetRaised = 0;
            var cellsResetRaised = 0;

            Assert.Equal(10, target.Rows.Count);

            target.Cells.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    ++cellsResetRaised;
            };

            target.Rows.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    ++rowsResetRaised;
            };

            target.Items = CreateData(20);

            Assert.Equal(20, target.Rows.Count);
            Assert.Equal(1, cellsResetRaised);
            Assert.Equal(1, rowsResetRaised);
        }

        [Fact]
        public void Disposing_Releases_Listeners_On_Items()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);
            Assert.Equal(20, target.Cells.Count);
            Assert.Equal(1, data.CollectionChangedSubscriberCount());

            foreach (var item in data)
            {
                Assert.Equal(2, item.PropertyChangedSubscriberCount());
            }

            target.Dispose();

            Assert.Equal(0, target.Rows.Count);
            Assert.Equal(0, target.Cells.Count);
            Assert.Equal(0, data.CollectionChangedSubscriberCount());

            foreach (var item in data)
            {
                Assert.Equal(0, item.PropertyChangedSubscriberCount());
            }
        }
#endif

        public class Sorted
        {
#if false
            [Fact]
            public void Sorts_Initial_Cells()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(10, target.Rows.Count);

                AssertCells(target.Cells, data);
            }

            [Fact]
            public void Creates_Cells_For_Added_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                AssertCells(target.Cells, data);

                data.Add(new Row { Id = 10, Caption = "New Row 10" });

                Assert.Equal(11, target.Rows.Count);
                AssertCells(target.Cells, data);
            }

            [Fact]
            public void Removes_Cells_For_Removed_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                AssertCells(target.Cells, data);

                data.RemoveAt(5);

                Assert.Equal(9, target.Rows.Count);
                AssertCells(target.Cells, data);
            }

            [Fact]
            public void Updates_Cells_For_Replaced_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(10, target.Rows.Count);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;
                
                data[5] = new Row { Id = 10, Caption = "New Row 10" };

                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(3, raised);

                var cell0 = Assert.IsType<TextCell<int>>(target.Cells[0, 5]);
                var cell1 = Assert.IsType<TextCell<string>>(target.Cells[1, 5]);

                AssertCells(target.Cells, data);
            }

            [Fact]
            public void Updates_Cells_For_Moved_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(10, target.Rows.Count);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.Move(5, 8);

                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(3, raised);

                AssertCells(target.Cells, data);
            }

            [Fact]
            public void Updates_Cells_For_Clear()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(10, target.Rows.Count);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.Clear();

                Assert.Equal(0, target.Rows.Count);
                Assert.Equal(1, raised);

                AssertCells(target.Cells, data);
            }
#endif

            [Fact]
            void Raises_Rows_Reset_When_Reassigning_Items()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var raised = 0;

                target.Rows.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Reset)
                        ++raised;
                };

                target.Items = CreateData();

                Assert.Equal(1, raised);
            }

            private static FlatTreeDataGridSource<Row> CreateTarget(IEnumerable<Row> rows)
            {
                var result = FlatTreeDataGridSourceTests.CreateTarget(rows);
                result.Sort((x, y) => y.Id - x.Id);
                return result;
            }

#if false
            private static void AssertCells(ICells cells, IList<Row> data)
            {
                var sortedData = data.OrderByDescending(x => x.Id).ToList();
                FlatTreeDataGridSourceTests.AssertCells(cells, sortedData);
            }
#endif
        }

        private static FlatTreeDataGridSource<Row> CreateTarget(IEnumerable<Row> rows)
        {
            return new FlatTreeDataGridSource<Row>(rows)
            {
                Columns =
                {
                    new TextColumn<Row, int>("ID", x => x.Id),
                    new TextColumn<Row, string?>("Caption", x => x.Caption),
                }
            };
        }

        private static AvaloniaList<Row> CreateData(int count = 10)
        {
            var rows = Enumerable.Range(0, count).Select(x => new Row { Id = x, Caption = $"Row {x}" });
            return new AvaloniaList<Row>(rows);
        }
#if false
        private static void AssertCells(ICells cells, IList<Row> data)
        {
            Assert.Equal(data.Count, cells.RowCount);
            Assert.Equal(data.Count * cells.ColumnCount, cells.Count);

            for (var i = 0; i < data.Count; ++i)
            {
                var cell0 = Assert.IsType<TextCell<int>>(cells[0, i]);
                var cell1 = Assert.IsType<TextCell<string>>(cells[1, i]);
                Assert.Equal(data[i].Id, cell0.Value);
                Assert.Equal(data[i].Caption, cell1.Value);
            }
        }
#endif
        private class Row : NotifyingBase
        {
            private int _id;
            private string? _caption;

            public int Id 
            {
                get => _id;
                set => RaiseAndSetIfChanged(ref _id, value);
            }

            public string? Caption 
            {
                get => _caption;
                set => RaiseAndSetIfChanged(ref _caption, value);
            }
        }
    }
}
