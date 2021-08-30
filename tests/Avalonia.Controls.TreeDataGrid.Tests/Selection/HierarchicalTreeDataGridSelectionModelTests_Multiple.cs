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
        public class SelectedIndex
        {
            [Fact]
            public void Can_Set_Root_SelectedIndex()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0", e.SelectedItems.Single().Caption);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(0);

                Assert.Equal(1, raised);
                Assert.Equal(new IndexPath(0), target.SelectedIndex);
                Assert.Equal(new IndexPath(0), target.SelectedIndexes.Single());
                Assert.Equal("Node 0", target.SelectedItem!.Caption);
                Assert.Equal("Node 0", target.SelectedItems.Single()!.Caption);
            }

            [Fact]
            public void Can_Set_Child_SelectedIndex()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single().Caption);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(0, 2);

                Assert.Equal(1, raised);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
            }

            [Fact]
            public void Setting_SelectedIndex_Clears_Old_Selection()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 1);
                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(0, 1), e.DeselectedIndexes.Single());
                    Assert.Equal("Node 0-1", e.DeselectedItems.Single().Caption);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single().Caption);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(0, 2);

                Assert.Equal(1, raised);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
            }

            [Fact]
            public void Can_Set_SelectedIndex_To_Empty()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(0, 2), e.DeselectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.DeselectedItems.Single().Caption);
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.SelectedIndex = default;

                Assert.Equal(1, raised);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
            }

            [Fact]
            public void Out_Of_Range_SelectedIndex_Clears_Selection()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(0, 2), e.DeselectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.DeselectedItems.Single().Caption);
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(5, 10, 250);

                Assert.Equal(1, raised);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
            }

            [Fact]
            public void Out_Of_Range_By_Depth_SelectedIndex_Clears_Selection()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(0, 2), e.DeselectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.DeselectedItems.Single().Caption);
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(0, 2, 3);

                Assert.Equal(1, raised);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
            }

            [Fact]
            public void Can_Select_Unexpanded_Item()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(1, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 1-2", e.SelectedItems.Single().Caption);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(1, 2);

                Assert.Equal(1, raised);
                Assert.Equal(new IndexPath(1, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(1, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 1-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 1-2", target.SelectedItems.Single()!.Caption);
            }

            [Fact]
            public void Setting_SelectedIndex_During_CollectionChanged_Results_In_Correct_Selection()
            {
                var data = new AvaloniaList<Node>();
                var source = CreateSource(data, expandRoot: false);
                var target = CreateTarget(source);
                var binding = new MockBinding(target, data);

                data.Add(new Node());

                Assert.Equal(new IndexPath(0), target.SelectedIndex);
            }

            [Fact]
            public void PropertyChanged_Is_Raised()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++raised;
                    }
                };

                target.SelectedIndex = new IndexPath(0, 2);

                Assert.Equal(1, raised);
            }

            private class MockBinding : ICollectionChangedListener
            {
                private readonly HierarchicalTreeDataGridSelectionModel<Node> _target;

                public MockBinding(HierarchicalTreeDataGridSelectionModel<Node> target, AvaloniaList<Node> data)
                {
                    _target = target;
                    CollectionChangedEventManager.Instance.AddListener(data, this);
                }

                public void Changed(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e)
                {
                    _target.Select(new IndexPath(0));
                }

                public void PostChanged(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e)
                {
                }

                public void PreChanged(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e)
                {
                }
            }
        }

        public class SelectedIndexes
        {
            [Fact]
            public void PropertyChanged_Is_Raised_When_SelectedIndex_Changes()
            {
                var target = CreateTarget();
                var raised = 0;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndexes))
                    {
                        ++raised;
                    }
                };

                target.SelectedIndex = new IndexPath(1);

                Assert.Equal(1, raised);
            }
        }

        public class SelectedItem
        {
            [Fact]
            public void PropertyChanged_Is_Raised_When_SelectedIndex_Changes()
            {
                var target = CreateTarget();
                var raised = 0;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedItem))
                    {
                        ++raised;
                    }
                };

                target.SelectedIndex = new IndexPath(1);

                Assert.Equal(1, raised);
            }
        }

        public class Select
        {
            [Fact]
            public void Select_Sets_SelectedIndex_If_Previously_Unset()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single().Caption);
                    ++raised;
                };

                target.Select(new IndexPath(0, 2));

                Assert.Equal(1, raised);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-2", target.SelectedItems.Single().Caption);
            }

            [Fact]
            public void Select_Adds_To_Selection()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.Select(new IndexPath(0));

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single().Caption);
                    ++raised;
                };

                target.Select(new IndexPath(0, 2));

                Assert.Equal(1, raised);
                Assert.Equal(new IndexPath(0), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(0), new IndexPath(0, 2) }, target.SelectedIndexes);
                Assert.Equal("Node 0", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0", "Node 0-2" }, target.SelectedItems.Select(x => x.Caption));
            }

            [Fact]
            public void Select_With_Invalid_Index_Does_Nothing()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) => ++raised;

                target.Select(new IndexPath(5, 10, 250));

                Assert.Equal(0, raised);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-2", target.SelectedItems.Single().Caption);
            }

            [Fact]
            public void Selecting_Already_Selected_Item_Doesnt_Raise_SelectionChanged()
            {
                var data = CreateData();
                var source = CreateSource(data);
                var target = CreateTarget(source);
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) => ++raised;

                target.Select(new IndexPath(0, 2));

                Assert.Equal(0, raised);
            }
        }

        public class Deselect
        {
            [Fact]
            public void Deselect_Clears_Selected_Item()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0);
                target.Select(new IndexPath(0, 1));

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new[] { new IndexPath(0, 1) }, e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 0-1" }, e.DeselectedItems.Select(x => x.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.Deselect(new IndexPath(0, 1));

                Assert.Equal(new IndexPath(0), target.SelectedIndex);
                Assert.Equal(new IndexPath(0), target.SelectedIndexes.Single());
                Assert.Equal("Node 0", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0" }, target.SelectedItems.Select(x => x.Caption));
                Assert.Equal(1, raised);
            }

            [Fact]
            public void Deselect_Updates_SelectedItem_To_First_Selected_Item()
            {
                var target = CreateTarget();

                target.Select(new IndexPath(0, 3));
                target.Select(new IndexPath(0, 4));
                target.Select(new IndexPath(0, 5));
                target.Deselect(new IndexPath(0, 3));

                Assert.Equal(new IndexPath(0, 4), target.SelectedIndex);
            }
        }

        public class Clear
        {
            [Fact]
            public void Clear_Raises_SelectionChanged()
            {
                var target = CreateTarget();
                var raised = 0;

                target.Select(new IndexPath(0, 1));
                target.Select(new IndexPath(0, 2));

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new[] { new IndexPath(0, 1), new IndexPath(0, 2) }, e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 0-1", "Node 0-2" }, e.DeselectedItems.Select(x => x.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.Clear();

                Assert.Equal(1, raised);
            }
        }

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
                Assert.Equal("Node 0-2", target.SelectedItems.Single().Caption);
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
                Assert.Equal("Node 0-2", target.SelectedItems.Single().Caption);
                Assert.Empty(rowSelection.SelectedIndexes);
                Assert.Equal(0, raised);

                SetExpanded(source, new IndexPath(0), true);

                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItems.Single().Caption);
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
                Assert.Equal("Node 0-0-1", target.SelectedItems.Single().Caption);
                Assert.Empty(rowSelection.SelectedIndexes);
                Assert.Equal(0, raised);

                SetExpanded(source, new IndexPath(0), true);

                Assert.Equal(new IndexPath(0, 0, 1), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-0-1", target.SelectedItems.Single().Caption);
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
