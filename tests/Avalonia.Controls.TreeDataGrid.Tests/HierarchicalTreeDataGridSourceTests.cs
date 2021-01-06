using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Xunit;

namespace Avalonia.Controls.TreeDataGrid.Tests
{
    public class HierarchicalTreeDataGridSourceTests
    {
        [Fact]
        public void Creates_Cells_For_Root_Models()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(5, target.Rows.Count);
            AssertCells(target.Cells, 5, 0, data);
        }

        [Fact]
        public void Expanding_Root_Node_Creates_Child_Cells()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            var cell0 = Assert.IsType<ExpanderCell<Node, int>>(target.Cells[0, 0]);
            cell0.IsExpanded = true;

            Assert.Equal(10, target.Rows.Count);
            AssertCells(target.Cells, 10, 0, data.Take(1));
            AssertCells(target.Cells, 10, 1, data[0].Children!);
            AssertCells(target.Cells, 10, 6, data.Skip(1));
        }

        [Fact]
        public void Attempting_To_Expand_Node_That_Has_No_Children_Hides_Expander()
        {
            var data = new Node { Id = 0, Caption = "Node 0" };

            // Here we return true from hasChildren selector, but there are actually no children.
            // This may happen if calculating the children is expensive.
            var target = new HierarchicalTreeDataGridSource<Node>(data, x => x.Children, x => true);
            CreateColumns(target);

            var expander = (IExpanderCell)target.Cells[0, 0];
            Assert.True(expander.ShowExpander);

            expander.IsExpanded = true;

            Assert.False(expander.ShowExpander);
            Assert.False(expander.IsExpanded);
        }

        [Fact]
        public void Collapsing_Root_Node_Removes_Child_Cells()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var expander = (IExpanderCell)target.Cells[0, 0];

            expander.IsExpanded = true;
            Assert.Equal(10, target.Rows.Count);
            Assert.Equal(10, target.Cells.RowCount);

            expander.IsExpanded = false;
            Assert.Equal(5, target.Rows.Count);
            AssertCells(target.Cells, 5, 0, data);
        }

        [Fact]
        public void Creates_Cells_For_Added_Root_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(5, target.Rows.Count);
            AssertCells(target.Cells, 5, 0, data);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data.Add(new Node { Id = 100, Caption = "New Node 1" });

            Assert.Equal(6, target.Rows.Count);
            AssertCells(target.Cells, 6, 0, data);
            Assert.Equal(2, raised);
        }

        [Fact]
        public void Creates_Cells_For_Added_Child_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var expander = (IExpanderCell)target.Cells[0, 0];

            expander.IsExpanded = true;
            Assert.Equal(10, target.Rows.Count);
            Assert.Equal(10, target.Cells.RowCount);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data[0].Children!.Add(new Node { Id = 100, Caption = "New Node 1" });

            Assert.Equal(11, target.Rows.Count);
            Assert.Equal(11, target.Cells.RowCount);
            AssertCells(target.Cells, 11, 0, data.Take(1));
            AssertCells(target.Cells, 11, 1, data[0].Children!);
            AssertCells(target.Cells, 11, 7, data.Skip(1));
            Assert.Equal(2, raised);
        }

        [Fact]
        public void Removes_Cells_For_Removed_Root_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(5, target.Rows.Count);
            AssertCells(target.Cells, 5, 0, data);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data.RemoveAt(3);

            Assert.Equal(4, target.Rows.Count);
            AssertCells(target.Cells, 4, 0, data);
            Assert.Equal(1, raised);
        }

        [Fact]
        public void Removes_Cells_For_Removed_Root_Rows_With_Expanded_Rows()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var expander = (IExpanderCell)target.Cells[0, 2];

            expander.IsExpanded = true;
            Assert.Equal(10, target.Rows.Count);
            Assert.Equal(10, target.Cells.RowCount);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data.RemoveRange(1, 3);

            Assert.Equal(2, target.Rows.Count);
            AssertCells(target.Cells, 2, 0, data);
            Assert.Equal(1, raised);
        }

        [Fact]
        public void Updates_Cells_For_Replaced_Root_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(5, target.Rows.Count);
            AssertCells(target.Cells, 5, 0, data);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data[2] = new Node { Id = 100, Caption = "Replaced" };

            Assert.Equal(5, target.Rows.Count);
            AssertCells(target.Cells, 5, 0, data);
        }

        [Fact]
        public void Updates_Cells_For_Moved_Root_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(5, target.Rows.Count);
            AssertCells(target.Cells, 5, 0, data);

            var raised = 0;
            target.Cells.CollectionChanged += (s, e) => ++raised;

            data.Move(2, 4);

            Assert.Equal(5, target.Rows.Count);
            AssertCells(target.Cells, 5, 0, data);
        }

        public class Sorted
        {
            [Fact]
            public void Creates_Cells_For_Root_Models()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(5, target.Rows.Count);
                AssertCells(target.Cells, 5, 0, data);
            }

            [Fact]
            public void Expanding_Root_Node_Creates_Child_Cells()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                var cell0 = Assert.IsType<ExpanderCell<Node, int>>(target.Cells[0, 0]);
                cell0.IsExpanded = true;

                Assert.Equal(10, target.Rows.Count);
                AssertCells(target.Cells, 10, 0, data.TakeLast(1));
                AssertCells(target.Cells, 10, 1, data[4].Children!);
                AssertCells(target.Cells, 10, 6, data.SkipLast(1));
            }

            [Fact]
            public void Creates_Cells_For_Added_Root_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(5, target.Rows.Count);
                AssertCells(target.Cells, 5, 0, data);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.Add(new Node { Id = 100, Caption = "New Node 1" });

                Assert.Equal(6, target.Rows.Count);
                AssertCells(target.Cells, 6, 0, data);
                Assert.Equal(2, raised);
            }

            [Fact]
            public void Creates_Cells_For_Added_Child_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var expander = (IExpanderCell)target.Cells[0, 0];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data[4].Children!.Add(new Node { Id = 100, Caption = "New Node 1" });

                Assert.Equal(11, target.Rows.Count);
                Assert.Equal(11, target.Cells.RowCount);
                AssertCells(target.Cells, 11, 0, data.TakeLast(1));
                AssertCells(target.Cells, 11, 1, data[4].Children!);
                AssertCells(target.Cells, 11, 7, data.SkipLast(1));
                Assert.Equal(2, raised);
            }

            [Fact]
            public void Removes_Cells_For_Removed_Root_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(5, target.Rows.Count);
                AssertCells(target.Cells, 5, 0, data);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.RemoveAt(3);

                Assert.Equal(4, target.Rows.Count);
                AssertCells(target.Cells, 4, 0, data);
                Assert.Equal(1, raised);
            }

            [Fact]
            public void Removes_Cells_For_Removed_Root_Rows_With_Expanded_Rows()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var expander = (IExpanderCell)target.Cells[0, 2];

                expander.IsExpanded = true;
                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(10, target.Cells.RowCount);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.RemoveRange(1, 3);

                Assert.Equal(2, target.Rows.Count);
                AssertCells(target.Cells, 2, 0, data);
                Assert.Equal(3, raised);
            }

            [Fact]
            public void Updates_Cells_For_Replaced_Root_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(5, target.Rows.Count);
                AssertCells(target.Cells, 5, 0, data);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data[2] = new Node { Id = 100, Caption = "Replaced" };

                Assert.Equal(5, target.Rows.Count);
                AssertCells(target.Cells, 5, 0, data);
            }

            [Fact]
            public void Updates_Cells_For_Moved_Root_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(5, target.Rows.Count);
                AssertCells(target.Cells, 5, 0, data);

                var raised = 0;
                target.Cells.CollectionChanged += (s, e) => ++raised;

                data.Move(2, 4);

                Assert.Equal(5, target.Rows.Count);
                AssertCells(target.Cells, 5, 0, data);
            }

            private static HierarchicalTreeDataGridSource<Node> CreateTarget(IEnumerable<Node> rows)
            {
                var result = HierarchicalTreeDataGridSourceTests.CreateTarget(rows);
                result.Sort((x, y) => y.Id - x.Id);
                return result;
            }

            private static void AssertCells(ICells cells, int totalRowCount, int startRowIndex, IEnumerable<Node> data)
            {
                var sortedData = data.OrderByDescending(x => x.Id).ToList();
                HierarchicalTreeDataGridSourceTests.AssertCells(cells, totalRowCount, startRowIndex, sortedData);
            }
        }

        private static AvaloniaList<Node> CreateData()
        {
            var id = 0;
            var result = new AvaloniaList<Node>();

            for (var i = 0; i < 5; ++i)
            {
                var node = new Node
                {
                    Id = id++,
                    Caption = $"Node {i}",
                    Children = new AvaloniaList<Node>(),
                };

                result.Add(node);

                for (var j = 0; j < 5; ++j)
                {
                    node.Children.Add(new Node
                    {
                        Id = id++,
                        Caption = $"Node {i}-{j}",
                        Children = new AvaloniaList<Node>(),
                    });
                }
            }

            return result;
        }

        private static HierarchicalTreeDataGridSource<Node> CreateTarget(Node root)
        {
            var result = new HierarchicalTreeDataGridSource<Node>(
                root,
                x => x.Children,
                x => x.Children?.Count > 0);
            CreateColumns(result);
            return result;
        }

        private static HierarchicalTreeDataGridSource<Node> CreateTarget(IEnumerable<Node> roots)
        {
            var result = new HierarchicalTreeDataGridSource<Node>(
                roots,
                x => x.Children,
                x => x.Children?.Count > 0);
            CreateColumns(result);
            return result;
        }

        private static void CreateColumns(HierarchicalTreeDataGridSource<Node> source)
        {
            source.AddColumn("ID", x => x.Id);
            source.AddColumn("Caption", x => x.Caption);
        }

        private static void AssertCells(ICells cells, int totalRowCount, int startRowIndex, IEnumerable<Node> data)
        {
            Assert.Equal(2, cells.ColumnCount);
            Assert.Equal(totalRowCount, cells.RowCount);

            var rowIndex = startRowIndex;
            var i = 0;

            foreach (var model in data)
            {
                var cell0 = Assert.IsType<ExpanderCell<Node, int>>(cells[0, rowIndex]);
                var cell1 = Assert.IsType<TextCell<string>>(cells[1, rowIndex]);

                Assert.Equal(model.Id, cell0.Value);
                Assert.Equal(model.Children?.Count > 0, cell0.ShowExpander);
                Assert.Equal(model.Caption, cell1.Value);

                ++rowIndex;
            }
        }

        private class Node
        {
            public int Id { get; set; }
            public string? Caption { get; set; }
            public AvaloniaList<Node>? Children { get; set; }
        }
    }
}
