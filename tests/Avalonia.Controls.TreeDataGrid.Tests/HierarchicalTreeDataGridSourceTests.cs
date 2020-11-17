using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Xunit;

namespace Avalonia.Controls.TreeDataGrid.Tests
{
    public class HierarchicalTreeDataGridSourceTests
    {
        [Fact]
        public void Creates_Cells_For_Root_Model()
        {
            var data = new Node { Id = 0, Title = "Node 0", Description = "Root" };
            var target = CreateTarget(data);

            Assert.Equal(1, target.Cells.RowCount);
            Assert.Equal(3, target.Cells.ColumnCount);
            var cell0 = Assert.IsType<ExpanderCell<Node, int>>(target.Cells[0, 0]);
            var cell1 = Assert.IsType<TextCell<string>>(target.Cells[1, 0]);
            var cell2 = Assert.IsType<TextCell<string>>(target.Cells[2, 0]);
            Assert.Equal(0, cell0.Value);
            Assert.False(cell0.ShowExpander);
            Assert.Equal("Node 0", cell1.Value);
            Assert.Equal("Root", cell2.Value);
        }

        [Fact]
        public void Creates_Cells_For_Root_Models()
        {
            var data = new[]
            {
                new Node { Id = 0, Title = "Node 0", Description = "Root 0" },
                new Node { Id = 1, Title = "Node 1", Description = "Root 1" },
            };
            var target = CreateTarget(data);

            Assert.Equal(2, target.Cells.RowCount);
            Assert.Equal(3, target.Cells.ColumnCount);
            var cell0 = Assert.IsType<ExpanderCell<Node, int>>(target.Cells[0, 0]);
            var cell1 = Assert.IsType<TextCell<string>>(target.Cells[1, 0]);
            var cell2 = Assert.IsType<TextCell<string>>(target.Cells[2, 0]);
            Assert.Equal(0, cell0.Value);
            Assert.False(cell0.ShowExpander);
            Assert.Equal("Node 0", cell1.Value);
            Assert.Equal("Root 0", cell2.Value);

            cell0 = Assert.IsType<ExpanderCell<Node, int>>(target.Cells[0, 1]);
            cell1 = Assert.IsType<TextCell<string>>(target.Cells[1, 1]);
            cell2 = Assert.IsType<TextCell<string>>(target.Cells[2, 1]);
            Assert.Equal(1, cell0.Value);
            Assert.False(cell0.ShowExpander);
            Assert.Equal("Node 1", cell1.Value);
            Assert.Equal("Root 1", cell2.Value);
        }

        [Fact]
        public void Expanding_Root_Creates_Child_Cells()
        {
            var data = CreateTreeData();
            var target = CreateTarget(data);

            Assert.Equal(1, target.Cells.RowCount);
            Assert.Equal(3, target.Cells.ColumnCount);

            var cell0 = Assert.IsType<ExpanderCell<Node, int>>(target.Cells[0, 0]);
            Assert.True(cell0.ShowExpander);

            cell0.IsExpanded = true;

            Assert.Equal(6, target.Cells.RowCount);
            Assert.Equal(3, target.Cells.ColumnCount);
            Assert.Equal(6, target.Rows.Count);
            Assert.Equal(3, target.Columns.Count);

            var expanderCells = Enumerable.Range(0, target.Cells.RowCount)
                .Select(x => (ExpanderCell<Node, int>)target.Cells[0, x]);
            var ids = expanderCells.Select(x => x.Value).ToList();
            var modelIndexes = expanderCells.Select(x => x.ModelIndex).ToList();

            Assert.Equal(Enumerable.Range(0, 6), ids);
            Assert.Equal(
                new[] { new IndexPath(0) }.Concat(Enumerable.Range(0, 5).Select(x => new IndexPath(0, x))),
                modelIndexes);
        }

        [Fact]
        public void Attempting_To_Expand_Node_That_Has_No_Children_Hides_Expander()
        {
            var data = new Node { Id = 0, Title = "Node 0", Description = "Root" };

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
        public void Collapsing_Root_Removes_Child_Cells()
        {
            var data = CreateTreeData();
            var target = CreateTarget(data);
            var expander = (IExpander)target.Cells[0, 0];

            expander.IsExpanded = true;
            Assert.Equal(6, target.Cells.RowCount);

            expander.IsExpanded = false;
            Assert.Equal(1, target.Cells.RowCount);
        }

        private Node CreateTreeData()
        {
            var root = new Node 
            { 
                Id = 0, 
                Title = "Node 0", 
                Description = "Root",
                Children = new ObservableCollection<Node>(),
            };

            for (var i = 0; i < 5; ++i)
            {
                root.Children.Add(new Node
                {
                    Id = i + 1,
                    Title = $"Node 0-{i}",
                    Description = $"Child {i}",
                });
            }

            return root;
        }

        private HierarchicalTreeDataGridSource<Node> CreateTarget(Node root)
        {
            var result = new HierarchicalTreeDataGridSource<Node>(
                root,
                x => x.Children,
                x => x.Children?.Count > 0);
            CreateColumns(result);
            return result;
        }

        private HierarchicalTreeDataGridSource<Node> CreateTarget(IEnumerable<Node> roots)
        {
            var result = new HierarchicalTreeDataGridSource<Node>(
                roots,
                x => x.Children,
                x => x.Children?.Count > 0);
            CreateColumns(result);
            return result;
        }

        private void CreateColumns(HierarchicalTreeDataGridSource<Node> source)
        {
            source.AddColumn("ID", x => x.Id);
            source.AddColumn("Title", x => x.Title);
            source.AddColumn("Description", x => x.Description);
        }

        private class Node
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public ObservableCollection<Node>? Children { get; set; }
        }
    }
}
