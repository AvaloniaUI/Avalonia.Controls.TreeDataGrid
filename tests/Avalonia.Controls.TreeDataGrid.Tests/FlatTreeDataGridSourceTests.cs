using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Xunit;

namespace Avalonia.Controls.TreeDataGrid.Tests
{
    public class FlatTreeDataGridSourceTests
    {
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

        public class Sorted
        {
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

            private static FlatTreeDataGridSource<Row> CreateTarget(IEnumerable<Row> rows)
            {
                var result = FlatTreeDataGridSourceTests.CreateTarget(rows);
                result.Sort((x, y) => y.Id - x.Id);
                return result;
            }

            private static void AssertCells(ICells cells, IList<Row> data)
            {
                var sortedData = data.OrderByDescending(x => x.Id).ToList();
                FlatTreeDataGridSourceTests.AssertCells(cells, sortedData);
            }
        }


        private static FlatTreeDataGridSource<Row> CreateTarget(IEnumerable<Row> rows)
        {
            var result = new FlatTreeDataGridSource<Row>(rows);
            CreateColumns(result);
            return result;
        }

        private static void CreateColumns(FlatTreeDataGridSource<Row> source)
        {
            source.AddColumn("ID", x => x.Id);
            source.AddColumn("Caption", x => x.Caption);
        }

        private static ObservableCollection<Row> CreateData()
        {
            var rows = Enumerable.Range(0, 10).Select(x => new Row { Id = x, Caption = $"Row {x}" });
            return new ObservableCollection<Row>(rows);
        }

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

        private class Row
        {
            public int Id { get; set; }
            public string? Caption { get; set; }
        }
    }
}
