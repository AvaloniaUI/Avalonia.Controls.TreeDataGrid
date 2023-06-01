using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests
{
    public class HierarchicalTreeDataGridSourceTests
    {
        public class RowsAndCells
        {
            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Creates_Cells_For_Root_Models(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                AssertState(target, data, 5, sorted);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Expanding_Root_Node_Creates_Child_Cells(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(0));

                AssertState(target, data, 10, sorted, new IndexPath(0));
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Collapsing_Root_Node_Removes_Child_Cells(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(0));

                Assert.Equal(10, target.Rows.Count);

                target.Collapse(new IndexPath(0));

                AssertState(target, data, 5, sorted);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Adding_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data.Add(new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 6, sorted);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Inserting_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data.Insert(1, new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 6, sorted);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Removing_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data.RemoveAt(1);

                AssertState(target, data, 4, sorted);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Removing_Root_Row_With_Earlier_Row_Expanded_To_Grandchildren(bool sorted)
            {
                var data = CreateData();
                data[0].Children![0].Children = new AvaloniaListDebug<Node>
                {
                    new Node
                    {
                        Id = 100,
                        Caption = "Node 0-0-0",
                    }
                };

                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(0));
                target.Expand(new IndexPath(0, 0));

                Assert.Equal(11, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data.RemoveAt(1);

                AssertState(target, data, 10, sorted, new IndexPath(0), new IndexPath(0, 0));
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Removing_Root_Row_With_Later_Row_Expanded(bool sorted)
            {
                var data = CreateData();

                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(4));

                Assert.Equal(10, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data.RemoveAt(1);

                AssertState(target, data, 9, sorted, new IndexPath(3));
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Removing_Expanded_Root_Row_Unsubscribes_From_CollectionChanged(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var toRemove = data[1];

                target.Expand(1);
                Assert.Equal(1, toRemove.Children!.CollectionChangedSubscriberCount());

                data.RemoveAt(1);
                Assert.Equal(0, toRemove.Children!.CollectionChangedSubscriberCount());
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Removing_Expanded_Root_Row_With_Expanded_Child_Unsubscribes_From_CollectionChanged(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var toRemove = data[1].Children![1];

                toRemove.Children = new AvaloniaListDebug<Node> { new Node() };

                target.Expand(new IndexPath(1, 1));
                Assert.Equal(1, toRemove.Children!.CollectionChangedSubscriberCount());

                data.RemoveAt(1);
                Assert.Equal(0, toRemove.Children!.CollectionChangedSubscriberCount());
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Adding_Child_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(0));

                Assert.Equal(10, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data[0].Children!.Add(new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 11, sorted, new IndexPath(0));
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Adding_Child_To_Expanded_Then_Unexpanded_Root_Node(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(0));
                target.Collapse(new IndexPath(0));

                data[0].Children!.Add(new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 5, sorted);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Inserting_Child_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(0));

                Assert.Equal(10, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data[0].Children!.Insert(1, new Node { Id = 100, Caption = "New Node 1" });

                AssertState(target, data, 11, sorted, new IndexPath(0));
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Removing_Child_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(0));
                Assert.Equal(10, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data[0].Children!.RemoveAt(3);

                AssertState(target, data, 9, sorted, new IndexPath(0));
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Removing_Child_Rows_At_Start(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                target.Expand(new IndexPath(0));
                Assert.Equal(10, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data[0].Children!.RemoveRange(0, 2);

                AssertState(target, data, 8, sorted, new IndexPath(0));
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Replacing_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data[2] = new Node { Id = 100, Caption = "Replaced" };

                AssertState(target, data, 5, sorted);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Supports_Moving_Root_Row(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);

                Assert.Equal(5, target.Rows.Count);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) => ++raised;

                data.Move(2, 4);

                AssertState(target, data, 5, sorted);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Setting_Sort_Updates_Rows()
            {
                var data = CreateData();
                var target = CreateTarget(data, false);

                target.Expand(new IndexPath(0));

                Assert.Equal(10, target.Rows.Count);

                target.Sort((x, y) => y.Id - x.Id);

                AssertState(target, data, 10, true, new IndexPath(0));
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Clearing_Sort_Updates_Rows()
            {
                var data = CreateData();
                var target = CreateTarget(data, true);

                target.Expand(new IndexPath(0));

                Assert.Equal(10, target.Rows.Count);

                target.Sort(null);

                AssertState(target, data, 10, false, new IndexPath(0));
            }
        }

        public class Expansion
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Expanding_Updates_Cell_IsExpanded()
            {
                var data = CreateData();
                var target = CreateTarget(data, false);
                var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);
                var raised = 0;

                expander.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "IsExpanded")
                        ++raised;
                };

                target.Expand(new IndexPath(0));

                Assert.True(expander.IsExpanded);
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Expanding_Previously_Expanded_Node_Creates_Expanded_Descendent()
            {
                var data = CreateData();
                var target = CreateTarget(data, false);

                data[0].Children![0].Children = new AvaloniaListDebug<Node>
                {
                    new Node { Id = 100, Caption = "Grandchild" }
                };

                // Expand first root node.
                target.Expand(new IndexPath(0));

                AssertState(target, data, 10, false, new IndexPath(0));

                // Expand first child node.
                target.Expand(new IndexPath(0, 0));

                // Grandchild should now be visible.
                AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 0));

                // Collapse root node.
                target.Collapse(new IndexPath(0));
                AssertState(target, data, 5, false);

                // And expand again. Grandchild should now be visible once more.
                target.Expand(new IndexPath(0));
                AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 0));
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Shows_Expander_For_Row_With_Children()
            {
                var data = CreateData();
                var target = CreateTarget(data, false);
                var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

                Assert.True(expander.ShowExpander);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Hides_Expander_For_Row_Without_Children()
            {
                var data = new[] { new Node { Id = 0, Caption = "Node 0" } };
                var target = CreateTarget(data, false);
                var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

                Assert.False(expander.ShowExpander);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Attempting_To_Expand_Node_That_Has_No_Children_Hides_Expander()
            {
                var data = new Node { Id = 0, Caption = "Node 0" };

                // Here we return true from hasChildren selector, but there are actually no children.
                // This may happen if calculating the children is expensive.
                var target = new HierarchicalTreeDataGridSource<Node>(data)
                {
                    Columns =
                    {
                        new HierarchicalExpanderColumn<Node>(
                            new TextColumn<Node, int>("ID", x => x.Id),
                            x => x.Children,
                            x => true),
                        new TextColumn<Node, string?>("Caption", x => x.Caption),
                    }
                };

                var expander = (IExpanderCell)target.Rows.RealizeCell(target.Columns[0], 0, 0);

                target.Expand(new IndexPath(0));

                Assert.False(expander.ShowExpander);
                Assert.False(expander.IsExpanded);
            }
        }

        public class ExpansionBinding
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Root_Is_Initially_Expanded()
            {
                var data = CreateData();
                data[0].IsExpanded = true;

                var target = CreateTarget(data, false, bindExpanded: true);
                RealizeCells(target);

                AssertState(target, data, 10, false, new IndexPath(0));
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Child_Is_Initially_Expanded()
            {
                var data = CreateData();
                data[0].IsExpanded = true;
                data[0].Children![1].IsExpanded = true;
                data[0].Children![1].Children!.Add(new Node());

                var target = CreateTarget(data, false, bindExpanded: true);
                RealizeCells(target);

                AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 1));
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Handles_Initial_Expanded_Row_With_No_Children()
            {
                var data = CreateData();
                data[0].IsExpanded = true;

                // This node has no children.
                data[0].Children![1].IsExpanded = true;

                var target = CreateTarget(data, false, bindExpanded: true);
                RealizeCells(target);

                AssertState(target, data, 10, false, new IndexPath(0));
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Root_Can_Be_Expanded_Via_Model()
            {
                var data = CreateData();
                var target = CreateTarget(data, false, bindExpanded: true);

                RealizeCells(target);
                AssertState(target, data, 5, false);

                data[0].IsExpanded = true;

                AssertState(target, data, 10, false, new IndexPath(0));
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Child_Can_Be_Expanded_Via_Model()
            {
                var data = CreateData();
                data[0].Children![1].Children!.Add(new Node());

                var target = CreateTarget(data, false, bindExpanded: true);

                RealizeCells(target);
                AssertState(target, data, 5, false);

                data[0].IsExpanded = true;
                RealizeRow(target, new IndexPath(0, 1));
                data[0].Children![1].IsExpanded = true;

                AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 1));
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Expanding_Collapsing_Root_Row_Writes_To_Model()
            {
                var data = CreateData();
                var target = CreateTarget(data, false, bindExpanded: true);

                RealizeCells(target);
                AssertState(target, data, 5, false);

                ((IExpander)target.Rows[0]).IsExpanded = true;

                AssertState(target, data, 10, false, new IndexPath(0));

                ((IExpander)target.Rows[0]).IsExpanded = false;

                AssertState(target, data, 5, false);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Expanding_Collapsing_Child_Row_Writes_To_Model()
            {
                var data = CreateData();
                data[0].Children![1].Children!.Add(new Node());

                var target = CreateTarget(data, false, bindExpanded: true);

                RealizeCells(target);
                AssertState(target, data, 5, false);

                ((IExpander)target.Rows[0]).IsExpanded = true;
                ((IExpander)target.Rows[2]).IsExpanded = true;

                AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 1));

                ((IExpander)target.Rows[2]).IsExpanded = false;

                AssertState(target, data, 10, false, new IndexPath(0));
            }

            private static void AssertState(
                HierarchicalTreeDataGridSource<Node> target,
                IList<Node> data,
                int expectedRows,
                bool sorted,
                params IndexPath[] expanded)
            {
                HierarchicalTreeDataGridSourceTests.AssertState(target, data, expectedRows, sorted, expanded);
                AssertDataState(default, data, expanded);
            }

            private static void AssertDataState(IndexPath parentIndex, IList<Node> data, IndexPath[] expanded)
            {
                for (var i = 0; i < data.Count; ++i)
                {
                    var node = data[i];
                    var nodeIndex = parentIndex.Append(i);
                    Assert.Equal(expanded.Contains(nodeIndex), node.IsExpanded);

                    if (node.Children is not null)
                        AssertDataState(nodeIndex, node.Children, expanded);
                }
            }

            private static void RealizeCells(HierarchicalTreeDataGridSource<Node> target)
            {
                for (var c = 0; c < target.Columns.Count; c++)
                {
                    var column = target.Columns[c];
                    for (var r = 0; r < target.Rows.Count; ++r)
                        target.Rows.RealizeCell(column, c, r);
                }
            }

            private static void RealizeRow(
                HierarchicalTreeDataGridSource<Node> target,
                IndexPath modelIndex)
            {
                var rowIndex = target.Rows.ModelIndexToRowIndex(modelIndex);

                Assert.NotEqual(-1, rowIndex);

                for (var c = 0; c < target.Columns.Count; c++)
                {
                    var column = target.Columns[c];
                    target.Rows.RealizeCell(column, c, rowIndex);
                }
            }
        }

        public class ShowExpander
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Initially_Hides_Expander_With_No_Children()
            {
                var data = CreateData(count: 1, childCount: 0);
                var target = CreateTarget(data, false);
                var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

                Assert.False(expander.ShowExpander);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Initially_Shows_Expander_With_Children()
            {
                var data = CreateData(count: 1, childCount: 1);
                var target = CreateTarget(data, false);
                var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

                Assert.True(expander.ShowExpander);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Shows_Expander_When_First_Child_Added()
            {
                var data = CreateData(count: 1, childCount: 0);
                var target = CreateTarget(data, false);
                var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);
                var raised = 0;

                expander.PropertyChanged += (s, e) =>
                {
                    Assert.Equal("ShowExpander", e.PropertyName);
                    ++raised;
                };

                data[0].Children!.Add(new Node());

                Assert.True(expander.ShowExpander);
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Hides_Expander_When_Last_Child_Removed()
            {
                var data = CreateData(count: 1, childCount: 1);
                var target = CreateTarget(data, false);
                var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);
                var raised = 0;

                expander.PropertyChanged += (s, e) =>
                {
                    Assert.Equal("ShowExpander", e.PropertyName);
                    ++raised;
                };

                data[0].Children!.RemoveAt(0);

                Assert.False(expander.ShowExpander);
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Cell_Synchronizes_Row_ShowExpander()
            {
                var data = CreateData(count: 1, childCount: 1);
                var target = CreateTarget(data, false);
                var row = (HierarchicalRow<Node>)target.Rows[0];
                var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

                Assert.True(expander.ShowExpander);
                Assert.True(row.ShowExpander);

                data[0].Children!.RemoveAt(0);

                Assert.False(expander.ShowExpander);
                Assert.False(row.ShowExpander);
            }
        }

        public class Selection
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Reassigning_Source_Updates_Selection_Model_Source()
            {
                var data1 = CreateData();
                var data2 = CreateData(5);
                var target = CreateTarget(data1, false);

                // Ensure selection model is created.
                Assert.Same(data1, ((ITreeDataGridSelection?)target.RowSelection)!.Source);

                target.Items = data2;

                Assert.Same(data2, ((ITreeDataGridSelection?)target.RowSelection)!.Source);
            }
        }

        public class Items
        {
            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Can_Reassign_Items(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var rowsAddedRaised = 0;
                var rowsRemovedRaised = 0;

                Assert.Equal(5, target.Rows.Count);

                target.Rows.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        rowsAddedRaised += e.NewItems!.Count;
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                        rowsRemovedRaised += e.OldItems!.Count;
                };

                target.Items = CreateData(10);

                Assert.Equal(10, target.Rows.Count);
                Assert.Equal(5, rowsRemovedRaised);
                Assert.Equal(10, rowsAddedRaised);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Can_Reassign_Items_With_Expanded_Node(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var rowsAddedRaised = 0;
                var rowsRemovedRaised = 0;

                target.Expand(0);
                Assert.Equal(10, target.Rows.Count);

                target.Rows.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        rowsAddedRaised += e.NewItems!.Count;
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                        rowsRemovedRaised += e.OldItems!.Count;
                };

                target.Items = CreateData(12);

                Assert.Equal(12, target.Rows.Count);
                Assert.Equal(10, rowsRemovedRaised);
                Assert.Equal(12, rowsAddedRaised);
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Reassigning_Items_With_Expanded_Root_Node_Unsubscribes_From_CollectionChanged(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var toRemove = data[1];

                target.Expand(1);
                Assert.Equal(1, toRemove.Children!.CollectionChangedSubscriberCount());

                target.Items = CreateData(12);

                Assert.Equal(0, toRemove.Children!.CollectionChangedSubscriberCount());
            }

            [AvaloniaTheory(Timeout = 10000)]
            [InlineData(false)]
            [InlineData(true)]
            public void Reassigning_Items_With_Expanded_Child_Node_Unsubscribes_From_CollectionChanged(bool sorted)
            {
                var data = CreateData();
                var target = CreateTarget(data, sorted);
                var toRemove = data[1].Children![1];

                toRemove.Children = new AvaloniaListDebug<Node> { new Node() };

                target.Expand(new IndexPath(1, 1));
                Assert.Equal(1, toRemove.Children!.CollectionChangedSubscriberCount());

                target.Items = CreateData(12);

                Assert.Equal(0, toRemove.Children!.CollectionChangedSubscriberCount());
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Selects_Correct_Item_After_Items_Reassigned()
            {
                var data = CreateData();
                var target = CreateTarget(data, false);
                var raised = 0;

                target.RowSelection!.Select(new IndexPath(1, 0));

                var newData = CreateData(10);
                newData[1].Children![0].Caption = "New Selection";
                target.Items = newData;

                target.RowSelection!.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(1, 0), e.SelectedIndexes.Single());
                    Assert.Equal("New Selection", e.SelectedItems.Single()!.Caption);
                    ++raised;
                };

                target.RowSelection!.Select(new IndexPath(1, 0));

                Assert.Equal(1, raised);
            }
        }

        private static AvaloniaListDebug<Node> CreateData(int count = 5, int childCount = 5)
        {
            var id = 0;
            var result = new AvaloniaListDebug<Node>();

            for (var i = 0; i < count; ++i)
            {
                var node = new Node
                {
                    Id = id++,
                    Caption = $"Node {i}",
                    Children = new AvaloniaListDebug<Node>(),
                };

                result.Add(node);

                for (var j = 0; j < childCount; ++j)
                {
                    node.Children.Add(new Node
                    {
                        Id = id++,
                        Caption = $"Node {i}-{j}",
                        Children = new AvaloniaListDebug<Node>(),
                    });
                }
            }

            return result;
        }

        private static HierarchicalTreeDataGridSource<Node> CreateTarget(
            IEnumerable<Node> roots,
            bool sorted,
            bool bindExpanded = false)
        {
            var result = new HierarchicalTreeDataGridSource<Node>(roots)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<Node>(
                        new TextColumn<Node, int>("ID", x => x.Id),
                        x => x.Children,
                        null,
                        bindExpanded ? x => x.IsExpanded : null),
                    new TextColumn<Node, string?>("Caption", x => x.Caption),
                }
            };

            if (sorted)
                result.Sort((x, y) => y.Id - x.Id);

            return result;
        }

        private static void AssertState(
            HierarchicalTreeDataGridSource<Node> target,
            IList<Node> data,
            int expectedRows,
            bool sorted,
            params IndexPath[] expanded)
        {
            Assert.Equal(2, target.Columns.Count);
            Assert.Equal(expectedRows, target.Rows.Count);

            var rowIndex = 0;

            void AssertLevel(IndexPath parent, IList<Node> levelData)
            {
                var sortedData = levelData;

                if (sorted)
                {
                    var s = new List<Node>(levelData);
                    s.Sort((x, y) => y.Id - x.Id);
                    sortedData = s;
                }

                for (var i = 0; i < levelData.Count; ++i)
                {
                    var modelIndex = parent.Append(levelData.IndexOf(sortedData[i]));
                    var model = GetModel(data, modelIndex);
                    var row = Assert.IsType<HierarchicalRow<Node>>(target.Rows[rowIndex]);
                    var shouldBeExpanded = expanded.Contains(modelIndex);

                    Assert.Equal(modelIndex, row.ModelIndexPath);
                    Assert.True(
                        row.IsExpanded == shouldBeExpanded,
                        $"Expected index {modelIndex} IsExpanded == {shouldBeExpanded}");

                    ++rowIndex;

                    if (row.IsExpanded)
                    {
                        Assert.NotNull(model.Children);
                        AssertLevel(modelIndex, model.Children!);
                    }
                }
            }

            AssertLevel(default, data);
        }

        private static Node GetModel(IList<Node> data, IndexPath path)
        {
            var depth = path.Count;
            Node? node = null;

            if (depth == 0)
                throw new NotSupportedException();

            for (var i = 0; i < depth; ++i)
            {
                var j = path[i];
                node = node is null ? data[j] : node.Children![j];
            }

            return node!;
        }

        internal class Node : NotifyingBase
        {
            private int _id;
            private string? _caption;
            private AvaloniaListDebug<Node>? _children;
            private bool _isExpanded;

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

            public AvaloniaListDebug<Node>? Children 
            {
                get => _children;
                set => RaiseAndSetIfChanged(ref _children, value);
            }

            public bool IsExpanded
            {
                get => _isExpanded;
                set => RaiseAndSetIfChanged(ref _isExpanded, value);
            }
        }
    }
}
