using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Models;
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
            result.AddColumn("ID", x => x.Id);
            result.AddColumn("Title", x => x.Title);
            result.AddColumn("Description", x => x.Description);
            return result;
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
