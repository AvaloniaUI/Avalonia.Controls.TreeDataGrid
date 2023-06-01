using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Selection;
using Avalonia.Controls.TreeDataGridTests.Collections;
using Avalonia.Controls.Utils;
using Avalonia.Diagnostics;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Selection
{
    public class TreeSelectionModelBaseTests_Multiple
    {
        public class SelectedIndex
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Can_Set_SelectedIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single()!.Caption);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(0, 2);

                Assert.Equal(1, raised);
                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Set_Grandchild_SelectedIndex()
            {
                var data = CreateData(depth: 3);
                var target = CreateTarget(data);
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0, 0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-0-2", e.SelectedItems.Single()!.Caption);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(0, 0, 2);

                Assert.Equal(1, raised);
                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0, 0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-0-2", target.SelectedItems.Single()!.Caption);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Setting_SelectedIndex_Clears_Old_Selection()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 1);
                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(0, 1), e.DeselectedIndexes.Single());
                    Assert.Equal("Node 0-1", e.DeselectedItems.Single()!.Caption);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single()!.Caption);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(0, 2);

                Assert.Equal(1, raised);
                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Set_SelectedIndex_To_Empty()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(0, 2), e.DeselectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.DeselectedItems.Single()!.Caption);
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.SelectedIndex = default;

                Assert.Equal(1, raised);
                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Out_Of_Range_SelectedIndex_Clears_Selection()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(0, 2), e.DeselectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.DeselectedItems.Single()!.Caption);
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(5, 10, 250);

                Assert.Equal(1, raised);
                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Select_Unexpanded_Item()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(1, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 1-2", e.SelectedItems.Single()!.Caption);
                    ++raised;
                };

                target.SelectedIndex = new IndexPath(1, 2);

                Assert.Equal(1, raised);
                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(1, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(1, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 1-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 1-2", target.SelectedItems.Single()!.Caption);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Setting_SelectedIndex_During_CollectionChanged_Results_In_Correct_Selection()
            {
                var data = new AvaloniaList<Node>();
                var target = CreateTarget(data);
                _ = new MockBinding(target, data);

                data.Add(new Node());

                Assert.Equal(new IndexPath(0), target.SelectedIndex);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void PropertyChanged_Is_Raised()
            {
                var target = CreateTarget();
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
                private readonly TestTreeSelectionModel _target;

                public MockBinding(TestTreeSelectionModel target, AvaloniaList<Node> data)
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

        public class SelectedItem
        {
            [AvaloniaFact(Timeout = 10000)]
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

        public class SelectedIndexes
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Can_Get_Items_Via_Indexer()
            {
                var target = CreateTarget();

                target.Select(0);
                target.Select(1);
                target.Select(new IndexPath(1, 2));
                target.Select(new IndexPath(2, 3));

                Assert.Equal(4, target.Count);
                Assert.Equal(4, target.SelectedIndexes.Count);
                Assert.Equal(new IndexPath(0), target.SelectedIndexes[0]);
                Assert.Equal(new IndexPath(1), target.SelectedIndexes[1]);
                Assert.Equal(new IndexPath(1, 2), target.SelectedIndexes[2]);
                Assert.Equal(new IndexPath(2, 3), target.SelectedIndexes[3]);
            }

            [AvaloniaFact(Timeout = 10000)]
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

        public class SelectedItems
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Can_Get_Items_Via_Indexer()
            {
                var target = CreateTarget();

                target.Select(0);
                target.Select(1);
                target.Select(new IndexPath(1, 2));
                target.Select(new IndexPath(2, 3));

                Assert.Equal(4, target.Count);
                Assert.Equal(4, target.SelectedItems.Count);
                Assert.Equal("Node 0", target.SelectedItems[0]!.Caption);
                Assert.Equal("Node 1", target.SelectedItems[1]!.Caption);
                Assert.Equal("Node 1-2", target.SelectedItems[2]!.Caption);
                Assert.Equal("Node 2-3", target.SelectedItems[3]!.Caption);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void PropertyChanged_Is_Raised_When_SelectedIndex_Changes()
            {
                var target = CreateTarget();
                var raised = 0;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedItems))
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
            [AvaloniaFact(Timeout = 10000)]
            public void Select_Sets_SelectedIndex_If_Previously_Unset()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single()!.Caption);
                    ++raised;
                };

                target.Select(new IndexPath(0, 2));

                Assert.Equal(1, raised);
                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Select_Adds_To_Selection()
            {
                var target = CreateTarget();
                var raised = 0;

                target.Select(new IndexPath(0));

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Empty(e.DeselectedItems);
                    Assert.Equal(new IndexPath(0, 2), e.SelectedIndexes.Single());
                    Assert.Equal("Node 0-2", e.SelectedItems.Single()!.Caption);
                    ++raised;
                };

                target.Select(new IndexPath(0, 2));

                Assert.Equal(1, raised);
                Assert.Equal(2, target.Count);
                Assert.Equal(new IndexPath(0), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(0), new IndexPath(0, 2) }, target.SelectedIndexes);
                Assert.Equal("Node 0", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0", "Node 0-2" }, target.SelectedItems.Select(x => x?.Caption));
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Select_With_Invalid_Index_Does_Nothing()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) => ++raised;

                target.Select(new IndexPath(5, 10, 250));

                Assert.Equal(0, raised);
                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndexes.Single());
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal("Node 0-2", target.SelectedItems.Single()!.Caption);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Selecting_Already_Selected_Item_Doesnt_Raise_SelectionChanged()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 2);
                target.SelectionChanged += (s, e) => ++raised;

                target.Select(new IndexPath(0, 2));

                Assert.Equal(0, raised);
            }
        }

        public class Deselect
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Deselect_Clears_Selected_Item()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0);
                target.Select(new IndexPath(0, 1));

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new[] { new IndexPath(0, 1) }, e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 0-1" }, e.DeselectedItems.Select(x => x?.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.Deselect(new IndexPath(0, 1));

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0), target.SelectedIndex);
                Assert.Equal(new IndexPath(0), target.SelectedIndexes.Single());
                Assert.Equal("Node 0", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0" }, target.SelectedItems.Select(x => x?.Caption));
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Deselect_Updates_SelectedItem_To_First_Selected_Item()
            {
                var target = CreateTarget();

                target.Select(new IndexPath(0, 2));
                target.Select(new IndexPath(0, 3));
                target.Select(new IndexPath(0, 4));
                target.Deselect(new IndexPath(0, 2));

                Assert.Equal(2, target.Count);
                Assert.Equal(new IndexPath(0, 3), target.SelectedIndex);
            }
        }

        public class Clear
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Clear_Raises_SelectionChanged()
            {
                var target = CreateTarget();
                var raised = 0;

                target.Select(new IndexPath(0, 1));
                target.Select(new IndexPath(0, 2));

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new[] { new IndexPath(0, 1), new IndexPath(0, 2) }, e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 0-1", "Node 0-2" }, e.DeselectedItems.Select(x => x?.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.Clear();

                Assert.Equal(1, raised);
            }
        }

        public class AnchorIndex
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Setting_SelectedIndex_Sets_AnchorIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.AnchorIndex))
                    {
                        ++raised;
                    }
                };

                target.SelectedIndex = new IndexPath(0, 1);

                Assert.Equal(new IndexPath(0, 1), target.AnchorIndex);
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Setting_SelectedIndex_To_Empty_Doesnt_Clear_AnchorIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 1);

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.AnchorIndex))
                    {
                        ++raised;
                    }
                };

                target.SelectedIndex = default;

                Assert.Equal(new IndexPath(0, 1), target.AnchorIndex);
                Assert.Equal(0, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Select_Sets_AnchorIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 0);

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.AnchorIndex))
                    {
                        ++raised;
                    }
                };

                target.Select(new IndexPath(0, 1));

                Assert.Equal(new IndexPath(0, 1), target.AnchorIndex);
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Deselect_Doesnt_Clear_AnchorIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.Select(new IndexPath(0, 0));
                target.Select(new IndexPath(0, 1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.AnchorIndex))
                    {
                        ++raised;
                    }
                };

                target.Deselect(new IndexPath(0, 1));

                Assert.Equal(new IndexPath(0, 1), target.AnchorIndex);
                Assert.Equal(0, raised);
            }
        }

        public class RangeAnchorIndex
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Setting_SelectedIndex_Sets_RangeAnchorIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.RangeAnchorIndex))
                    {
                        ++raised;
                    }
                };

                target.SelectedIndex = new IndexPath(0, 1);

                Assert.Equal(new IndexPath(0, 1), target.RangeAnchorIndex);
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Setting_SelectedIndex_To_Empty_Doesnt_Clear_RangeAnchorIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 1);

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.AnchorIndex))
                    {
                        ++raised;
                    }
                };

                target.SelectedIndex = default;

                Assert.Equal(new IndexPath(0, 1), target.RangeAnchorIndex);
                Assert.Equal(0, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Select_Doesnt_Set_RangeAnchorIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 0);

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.AnchorIndex))
                    {
                        ++raised;
                    }
                };

                target.Select(new IndexPath(0, 1));

                Assert.Equal(new IndexPath(0, 0), target.RangeAnchorIndex);
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Deselect_Doesnt_Clear_RangeAnchorIndex()
            {
                var target = CreateTarget();
                var raised = 0;

                target.SelectedIndex = new IndexPath(0, 0);

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.AnchorIndex))
                    {
                        ++raised;
                    }
                };

                target.Deselect(new IndexPath(0, 0));

                Assert.Equal(new IndexPath(0, 0), target.RangeAnchorIndex);
                Assert.Equal(0, raised);
            }
        }

        public class SingleSelect
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Converting_To_Single_Selection_Removes_Multiple_Selection()
            {
                var target = CreateTarget();
                var raised = 0;

                target.Select(new IndexPath(0, 1));
                target.Select(new IndexPath(0, 2));
                target.Select(new IndexPath(0, 3));

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Equal(new[] { new IndexPath(0, 2), new IndexPath(0, 3) }, e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 0-2", "Node 0-3" }, e.DeselectedItems.Select(x => x?.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++raised;
                };

                target.SingleSelect = true;

                Assert.Equal(1, raised);
                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0, 1), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(0, 1) }, target.SelectedIndexes);
                Assert.Equal("Node 0-1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0-1" }, target.SelectedItems.Select(x => x?.Caption));
                Assert.Equal(1, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Raises_PropertyChanged()
            {
                var target = CreateTarget();
                var raised = 0;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SingleSelect))
                    {
                        ++raised;
                    }
                };

                target.SingleSelect = true;

                Assert.Equal(1, raised);
            }
        }

        public class CollectionChanges
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Adding_Root_Item_Before_Selected_Root_Item_Updates_Indexes()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var indexesChangedRaised = 0;
                var selectedIndexRaised = 0;

                target.SelectedIndex = new IndexPath(1);

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }
                };

                target.IndexesChanged += (s, e) =>
                {
                    Assert.Equal(default, e.ParentIndex);
                    Assert.Equal(0, e.StartIndex);
                    Assert.Equal(1, e.Delta);
                    ++indexesChangedRaised;
                };

                data.Insert(0, new Node { Caption = "new" });

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(2), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(2) }, target.SelectedIndexes);
                Assert.Equal("Node 1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 1" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(2), target.AnchorIndex);
                Assert.Equal(1, indexesChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(0, selectionChangedRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Adding_Child_Item_Before_Selected_Child_Item_Updates_Indexes()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var indexesChangedRaised = 0;
                var selectedIndexRaised = 0;

                target.SelectedIndex = new IndexPath(0, 1);

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }
                };

                target.IndexesChanged += (s, e) =>
                {
                    Assert.Equal(new IndexPath(0), e.ParentIndex);
                    Assert.Equal(0, e.StartIndex);
                    Assert.Equal(1, e.Delta);
                    ++indexesChangedRaised;
                };

                data[0].Children!.Insert(0, new Node { Caption = "new" });

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0, 2), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(0, 2) }, target.SelectedIndexes);
                Assert.Equal("Node 0-1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0-1" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(0, 2), target.AnchorIndex);
                Assert.Equal(1, indexesChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(0, selectionChangedRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Adding_Root_Item_Before_Selected_Child_Item_Updates_Indexes()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var indexesChangedRaised = 0;
                var selectedIndexRaised = 0;

                target.SelectedIndex = new IndexPath(0, 1);

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }
                };

                target.IndexesChanged += (s, e) =>
                {
                    Assert.Equal(default, e.ParentIndex);
                    Assert.Equal(0, e.StartIndex);
                    Assert.Equal(1, e.Delta);
                    ++indexesChangedRaised;
                };

                data.Insert(0, new Node { Caption = "new" });

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(1, 1), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(1, 1) }, target.SelectedIndexes);
                Assert.Equal("Node 0-1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0-1" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(1, 1), target.AnchorIndex);
                Assert.Equal(1, indexesChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(0, selectionChangedRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Adding_Root_Item_Before_Selected_Grandchild_Item_Updates_Indexes()
            {
                var data = CreateData(depth: 3);
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var indexesChangedRaised = 0;
                var selectedIndexRaised = 0;

                target.SelectedIndex = new IndexPath(0, 0, 1);

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }
                };

                target.IndexesChanged += (s, e) =>
                {
                    Assert.Equal(default, e.ParentIndex);
                    Assert.Equal(0, e.StartIndex);
                    Assert.Equal(1, e.Delta);
                    ++indexesChangedRaised;
                };

                data.Insert(0, new Node { Caption = "new" });

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(1, 0, 1), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(1, 0, 1) }, target.SelectedIndexes);
                Assert.Equal("Node 0-0-1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0-0-1" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(1, 0, 1), target.AnchorIndex);
                Assert.Equal(1, indexesChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(0, selectionChangedRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Adding_Root_Item_After_Selected_Root_Item_Doesnt_Raise_Events()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var raised = 0;

                target.SelectedIndex = new IndexPath(1);

                target.PropertyChanged += (s, e) => ++raised;
                target.SelectionChanged += (s, e) => ++raised;
                target.IndexesChanged += (s, e) => ++raised;

                data.Insert(2, new Node { Caption = "new" });

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(1), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(1) }, target.SelectedIndexes);
                Assert.Equal("Node 1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 1" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(1), target.AnchorIndex);
                Assert.Equal(0, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Removing_Root_Selected_Item_Updates_State()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;

                target.Select(new IndexPath(1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }
                };

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 1" }, e.DeselectedItems.Select(x => x!.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++selectionChangedRaised;
                };

                data.RemoveAt(1);

                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
                Assert.Equal(default, target.AnchorIndex);
                Assert.Equal(1, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Removing_Child_Selected_Item_Updates_State()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;

                target.Select(new IndexPath(0, 1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }
                };

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 0-1" }, e.DeselectedItems.Select(x => x!.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++selectionChangedRaised;
                };

                data[0].Children!.RemoveAt(1);

                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
                Assert.Equal(default, target.AnchorIndex);
                Assert.Equal(1, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Removing_Parent_Of_Selected_Item_Updates_State()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;

                target.Select(new IndexPath(0, 1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }
                };

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 0-1" }, e.DeselectedItems.Select(x => x!.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++selectionChangedRaised;
                };

                data.RemoveAt(0);

                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
                Assert.Equal(default, target.AnchorIndex);
                Assert.Equal(1, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Removing_Root_Item_Before_Selected_Root_Item_Updates_Indexes()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var indexesChangedraised = 0;

                target.SelectedIndex = new IndexPath(1);

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;

                target.IndexesChanged += (s, e) =>
                {
                    Assert.Equal(0, e.StartIndex);
                    Assert.Equal(-1, e.Delta);
                    ++indexesChangedraised;
                };

                data.RemoveAt(0);

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(0) }, target.SelectedIndexes);
                Assert.Equal("Node 1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 1" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(0), target.AnchorIndex);
                Assert.Equal(1, indexesChangedraised);
                Assert.Equal(0, selectionChangedRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Removing_Root_Item_Before_Selected_Child_Item_Updates_Indexes()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var indexesChangedraised = 0;

                target.SelectedIndex = new IndexPath(1, 1);

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;

                target.IndexesChanged += (s, e) =>
                {
                    Assert.Equal(0, e.StartIndex);
                    Assert.Equal(-1, e.Delta);
                    ++indexesChangedraised;
                };

                data.RemoveAt(0);

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0, 1), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(0, 1) }, target.SelectedIndexes);
                Assert.Equal("Node 1-1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 1-1" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(0, 1), target.AnchorIndex);
                Assert.Equal(1, indexesChangedraised);
                Assert.Equal(0, selectionChangedRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Removing_Child_Item_Before_Selected_Grandhild_Item_Updates_Indexes()
            {
                var data = CreateData(depth: 3);
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var indexesChangedraised = 0;

                target.SelectedIndex = new IndexPath(1, 1, 2);

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;

                target.IndexesChanged += (s, e) =>
                {
                    Assert.Equal(0, e.StartIndex);
                    Assert.Equal(-1, e.Delta);
                    ++indexesChangedraised;
                };

                data[1].Children!.RemoveAt(0);

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(1, 0, 2), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(1, 0, 2) }, target.SelectedIndexes);
                Assert.Equal("Node 1-1-2", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 1-1-2" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(1, 0, 2), target.AnchorIndex);
                Assert.Equal(1, indexesChangedraised);
                Assert.Equal(0, selectionChangedRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Removing_Child_Range_Updates_State()
            {
                var data = CreateData(depth: 3);
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var indexesChangedraised = 0;

                target.Select(new IndexPath(0, 1));
                target.Select(new IndexPath(0, 2));
                target.Select(new IndexPath(0, 3));

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 0-1" }, e.DeselectedItems.Select(x => x!.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++selectionChangedRaised;
                };

                target.IndexesChanged += (s, e) =>
                {
                    Assert.Equal(0, e.StartIndex);
                    Assert.Equal(-2, e.Delta);
                    ++indexesChangedraised;
                };

                data[0].Children!.RemoveRange(0, 2);

                Assert.Equal(2, target.Count);
                Assert.Equal(new IndexPath(0, 0), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(0, 0), new IndexPath(0, 1) }, target.SelectedIndexes);
                Assert.Equal("Node 0-2", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 0-2", "Node 0-3" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(0, 1), target.AnchorIndex);
                Assert.Equal(1, indexesChangedraised);
                Assert.Equal(1, selectionChangedRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Removing_Root_Item_After_Selected_Root_Item_Doesnt_Raise_Events()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var raised = 0;

                target.SelectedIndex = new IndexPath(1);

                target.PropertyChanged += (s, e) => ++raised;
                target.SelectionChanged += (s, e) => ++raised;
                target.IndexesChanged += (s, e) => ++raised;

                data.RemoveAt(2);

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(1), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(1) }, target.SelectedIndexes);
                Assert.Equal("Node 1", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 1" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(1), target.AnchorIndex);
                Assert.Equal(0, raised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Replacing_Selected_Root_Item_Updates_State()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;
                var selectedItemRaised = 0;

                target.Select(new IndexPath(1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }

                    if (e.PropertyName == nameof(target.SelectedItem))
                    {
                        ++selectedItemRaised;
                    }
                };

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 1" }, e.DeselectedItems.Select(x => x!.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++selectionChangedRaised;
                };

                data[1] = new Node { Caption = "new" };

                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
                Assert.Equal(1, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(1, selectedItemRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Replacing_Selected_Child_Item_Updates_State()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;
                var selectedItemRaised = 0;

                target.Select(new IndexPath(1, 1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }

                    if (e.PropertyName == nameof(target.SelectedItem))
                    {
                        ++selectedItemRaised;
                    }
                };

                target.SelectionChanged += (s, e) =>
                {
                    Assert.Empty(e.DeselectedIndexes);
                    Assert.Equal(new[] { "Node 1-1" }, e.DeselectedItems.Select(x => x!.Caption));
                    Assert.Empty(e.SelectedIndexes);
                    Assert.Empty(e.SelectedItems);
                    ++selectionChangedRaised;
                };

                data[1].Children![1] = new Node { Caption = "new" };

                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
                Assert.Equal(1, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(1, selectedItemRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Resetting_Root_Items_Clears_Selection()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;
                var selectedItemRaised = 0;
                var sourceResetRaised = 0;

                target.Select(new IndexPath(1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }

                    if (e.PropertyName == nameof(target.SelectedItem))
                    {
                        ++selectedItemRaised;
                    }
                };

                target.SelectionChanged += (s, e) =>++selectionChangedRaised;
                target.SourceReset += (s, e) =>
                {
                    Assert.Equal(default, e.ParentIndex);
                    ++sourceResetRaised;
                };

                data.Clear();

                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
                Assert.Equal(0, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(1, selectedItemRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Resetting_Child_Items_Clears_Selection()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;
                var selectedItemRaised = 0;
                var sourceResetRaised = 0;

                target.Select(new IndexPath(1, 1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }

                    if (e.PropertyName == nameof(target.SelectedItem))
                    {
                        ++selectedItemRaised;
                    }
                };

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;
                target.SourceReset += (s, e) =>
                {
                    Assert.Equal(new IndexPath(1), e.ParentIndex);
                    ++sourceResetRaised;
                };

                data[1].Children!.Clear();

                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
                Assert.Equal(0, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(1, selectedItemRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Resetting_Child_Items_Updates_SelectedItem_To_First_Selected_Item()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;
                var selectedItemRaised = 0;
                var sourceResetRaised = 0;

                target.Select(new IndexPath(1, 1));
                target.Select(new IndexPath(2, 1));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }

                    if (e.PropertyName == nameof(target.SelectedItem))
                    {
                        ++selectedItemRaised;
                    }
                };

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;
                target.SourceReset += (s, e) =>
                {
                    Assert.Equal(new IndexPath(1), e.ParentIndex);
                    ++sourceResetRaised;
                };

                data[1].Children!.Clear();

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(2, 1), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(2, 1) }, target.SelectedIndexes);
                Assert.Equal("Node 2-1", target.SelectedItem!.Caption);
                Assert.Equal("Node 2-1", target.SelectedItems.Single()!.Caption);
                Assert.Equal(0, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(1, selectedItemRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Resetting_Root_Items_To_Non_Empty_Collection_Clears_Selection()
            {
                var data = new ResettingCollection<Node>(CreateData());
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var selectedIndexRaised = 0;
                var selectedItemRaised = 0;
                var sourceResetRaised = 0;

                target.Select(new IndexPath(0));

                target.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(target.SelectedIndex))
                    {
                        ++selectedIndexRaised;
                    }

                    if (e.PropertyName == nameof(target.SelectedItem))
                    {
                        ++selectedItemRaised;
                    }
                };

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;
                target.SourceReset += (s, e) =>
                {
                    Assert.Equal(default, e.ParentIndex);
                    ++sourceResetRaised;
                };

                data.Reset(new[] { data[0] });

                Assert.Equal(0, target.Count);
                Assert.Equal(default, target.SelectedIndex);
                Assert.Empty(target.SelectedIndexes);
                Assert.Null(target.SelectedItem);
                Assert.Empty(target.SelectedItems);
                Assert.Equal(0, selectionChangedRaised);
                Assert.Equal(1, selectedIndexRaised);
                Assert.Equal(1, selectedItemRaised);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Doesnt_Crash_On_Removing_Last_Item_After_Resetting_To_Larger_Collection()
            {
                var data = new ResettingCollection<Node>(CreateData(depth: 4));
                var target = CreateTarget(data);

                target.Select(new IndexPath(0, 1, 2));
                data.Reset(data.Concat(new[] { new Node() }).ToList());
                data.RemoveAt(data.Count - 1);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Handles_Selection_Made_In_CollectionChanged()
            {
                // Tests the following scenario:
                //
                // - Items changes from empty to having 1 item
                // - ViewModel auto-selects item 0 in CollectionChanged
                // - SelectionModel receives CollectionChanged
                // - And so adjusts the selected item from 0 to 1, which is past the end of the items.
                //
                // There's not much we can do about this situation because the order in which
                // CollectionChanged handlers are called can't be known (the problem also exists with
                // WPF). The best we can do is not select an invalid index.
                var data = new AvaloniaList<Node>();
                var target = CreateTarget(data);

                data.CollectionChanged += (s, e) =>
                {
                    target.Select(new IndexPath(0));
                };

                data.Add(new Node { Caption = "foo" });

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(0), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(0) }, target.SelectedIndexes);
                Assert.Equal("foo", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "foo" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(0), target.AnchorIndex);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Clearing_Node_Selection_Unsubscribes_From_CollectionChanged()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                target.Select(new IndexPath(1, 1));

                var debug = (AvaloniaListDebug<Node>)data[1].Children!;
                Assert.Single(debug.GetCollectionChangedSubscribers());

                target.Deselect(new IndexPath(1, 1));

                Assert.Null(debug.GetCollectionChangedSubscribers());
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Clearing_Children_Updates_State()
            {
                var data = CreateData(depth: 4);
                var target = CreateTarget(data);
                var selectionChangedRaised = 0;
                var sourceResetRaised = 0;
                var indexesChangedraised = 0;

                target.Select(new IndexPath(0, 1));
                target.Select(new IndexPath(0, 1, 0));
                target.Select(new IndexPath(0, 1, 0, 1));
                target.Select(new IndexPath(0, 2));
                target.Select(new IndexPath(0, 3));
                target.Select(new IndexPath(1, 3));

                target.SelectionChanged += (s, e) => ++selectionChangedRaised;
                target.SourceReset += (s, e) => ++sourceResetRaised;
                target.IndexesChanged += (s, e) => ++indexesChangedraised;

                data[0].Children!.Clear();

                Assert.Equal(1, target.Count);
                Assert.Equal(new IndexPath(1, 3), target.SelectedIndex);
                Assert.Equal(new[] { new IndexPath(1, 3) }, target.SelectedIndexes);
                Assert.Equal("Node 1-3", target.SelectedItem!.Caption);
                Assert.Equal(new[] { "Node 1-3" }, target.SelectedItems.Select(x => x!.Caption));
                Assert.Equal(new IndexPath(1, 3), target.AnchorIndex);
                Assert.Equal(0, indexesChangedraised);
                Assert.Equal(0, selectionChangedRaised);
                Assert.Equal(1, sourceResetRaised);
            }
        }

        private static AvaloniaListDebug<Node> CreateNodes(IndexPath parentId, int depth = 2)
        {
            var result = new AvaloniaListDebug<Node>();

            for (var i = 0; i < 5; ++i)
            {
                var id = parentId.Append(i);

                var node = new Node
                {
                    Id = id,
                    Caption = "Node " + string.Join("-", id.ToArray()),
                    TargetDepth = depth,
                };

                result.Add(node);
            }

            return result;
        }

        private static AvaloniaListDebug<Node> CreateData(int depth = 2)
        {
            return CreateNodes(default, depth);
        }

        private static TestTreeSelectionModel CreateTarget(IList<Node>? data = null)
        {
            return new TestTreeSelectionModel(data ?? CreateData()) { SingleSelect = false };
        }

        private class Node
        {
            public IndexPath Id { get; set; }
            public string? Caption { get; set; }
            public AvaloniaList<Node>? Children { get; set; }
            public int TargetDepth { get; set; }
        }

        private class TestTreeSelectionModel : TreeSelectionModelBase<Node>
        {
            public TestTreeSelectionModel(IList<Node> data)
                : base(data)
            {
            }

            protected internal override IEnumerable<Node>? GetChildren(Node node)
            {
                if (node.Children is null && node.Id.Count < node.TargetDepth)
                    node.Children = CreateNodes(node.Id, node.TargetDepth);
                return node.Children;
            }
        }
    }
}
