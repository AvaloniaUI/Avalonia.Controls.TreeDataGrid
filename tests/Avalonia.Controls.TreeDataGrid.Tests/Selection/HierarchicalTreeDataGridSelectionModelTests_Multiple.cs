using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Utils;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests
{
    public class HierarchicalTreeDataGridSelectionModelTests_Multiple
    {
        public class RowSelection
        {
            [Fact]
            public void Handles_Changing_Row_Selection()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var rowSelection = ((ITreeDataGridSelectionModel)target).RowSelection;
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single().Caption);
                    ++raised;
                };

                rowSelection.SelectedIndex = 3;

                Assert.Equal(1, raised);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
            }
        }

        public class Collapse
        {
            [Fact]
            public void Remembers_Child_Selection_When_Row_Collapsed()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var rowSelection = ((ITreeDataGridSelectionModel)target).RowSelection;
                var raised = 0;

                rowSelection.SelectedIndex = 3;
                target.SelectionChanged += (s, e) => ++raised;
                SetExpanded(source, new IndexPath(0), false);

                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
                Assert.Empty(rowSelection.SelectedIndexes);
                Assert.Equal(0, raised);

                SetExpanded(source, new IndexPath(0), true);

                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
                Assert.Equal(3, rowSelection.SelectedIndex);
                Assert.Equal(0, raised);
            }

            [Fact]
            public void Remembers_Grandchild_Selection_When_Row_Collapsed()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var rowSelection = (ISelectionModel)target;
                var raised = 0;

                data[0].Children![0].Children = new AvaloniaList<Node>
            {
                new Node { Id = 100, Caption = "Node 0-0-0" },
                new Node { Id = 101, Caption = "Node 0-0-1" },
                new Node { Id = 102, Caption = "Node 0-0-2" },
            };

                SetExpanded(source, new IndexPath(0, 0), true);
                rowSelection.SelectedIndex = 3;
                target.SelectionChanged += (s, e) => ++raised;
                SetExpanded(source, new IndexPath(0), false);

                Assert.Equal(new IndexPath(0, 0, 1), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-0-1", target.SelectedItems.Single()!.Caption);
                Assert.Empty(rowSelection.SelectedIndexes);
                Assert.Equal(0, raised);

                SetExpanded(source, new IndexPath(0), true);

                Assert.Equal(new IndexPath(0, 0, 1), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-0-1", target.SelectedItems.Single()!.Caption);
                Assert.Equal(3, rowSelection.SelectedIndex);
                Assert.Equal(0, raised);
            }
        }

        private static HierarchicalTreeDataGridSelectionModel<Node> CreateTarget()
        {
            var data = CreateData();
            var source = CreateSource(data);
            return CreateTarget(source);
        }

        private static HierarchicalTreeDataGridSelectionModel<Node> CreateTarget(
            HierarchicalTreeDataGridSource<Node> source)
        {
            var result = (HierarchicalTreeDataGridSelectionModel<Node>)source.Selection;
            result.SingleSelect = false;
            return result;
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

        private static HierarchicalTreeDataGridSource<Node> CreateSource(IEnumerable<Node> roots, bool expandRoot = true)
        {
            var result = new HierarchicalTreeDataGridSource<Node>(roots)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<Node>(
                        new TextColumn<Node, int>("ID", x => x.Id),
                        x => x.Children,
                        x => x.Children?.Count > 0),
                    new TextColumn<Node, string?>("Caption", x => x.Caption),
                }
            };

            if (expandRoot)
                SetExpanded(result, new IndexPath(0), true);

            return result;
        }

        private static void SetExpanded(
            HierarchicalTreeDataGridSource<Node> source,
            IndexPath indexPath,
            bool expanded)
        {
            foreach (HierarchicalRow<Node> row in source.Rows)
            {
                if (row.ModelIndexPath == indexPath)
                {
                    row.IsExpanded = expanded;
                    return;
                }
            }

            throw new InvalidOperationException("Could not find row.");
        }

        internal class Node
        {
            public int Id { get; set; }
            public string? Caption { get; set; }
            public AvaloniaList<Node>? Children { get; set; }
        }
    }
}
