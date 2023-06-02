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
    public class FlatTreeDataGridSourceTests
    {
        [AvaloniaFact(Timeout = 10000)]
        public void Creates_Initial_Rows()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            AssertRows(target.Rows, data);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Supports_Adding_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
                Assert.Equal(10, e.NewStartingIndex);
                ++raised;
            };

            data.Add(new Row { Id = 10, Caption = "New Row 10" });

            Assert.Equal(11, target.Rows.Count);
            Assert.Equal(1, raised);

            AssertRows(target.Rows, data);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Supports_Removing_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action);
                Assert.Equal(5, e.OldStartingIndex);
                ++raised;
            };

            data.RemoveAt(5);

            Assert.Equal(1, raised);
            AssertRows(target.Rows, data);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Supports_Replacing_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Replace, e.Action);
                Assert.Equal(5, e.NewStartingIndex);
                Assert.Equal(5, e.OldStartingIndex);
                ++raised;
            };

            data[5] = new Row { Id = 10, Caption = "New Row 10" };

            Assert.Equal(1, raised);
            AssertRows(target.Rows, data);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Supports_Moving_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Move, e.Action);
                Assert.Equal(8, e.NewStartingIndex);
                Assert.Equal(5, e.OldStartingIndex);
                ++raised;
            };

            data.Move(5, 8);

            Assert.Equal(1, raised);
            AssertRows(target.Rows, data);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Supports_Clearing_Rows()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            Assert.Equal(10, target.Rows.Count);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
                ++raised;
            };

            data.Clear();

            Assert.Equal(1, raised);
            AssertRows(target.Rows, data);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Can_Reassign_Items()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var raised = 0;

            AssertRows(target.Rows, data);

            target.Rows.CollectionChanged += (s, e) =>
            {
                Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
                ++raised;
            };

            target.Items = data = CreateData(20);

            Assert.Equal(1, raised);
            AssertRows(target.Rows, data);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Raises_Rows_Reset_When_Reassigning_Items_But_Rows_Not_Yet_Read()
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

        public class Sorted
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Sorts_Initial_Cells()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                Assert.Equal(10, target.Rows.Count);

                AssertRows(target.Rows, data);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Supports_Adding_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                AssertRows(target.Rows, data);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) =>
                {
                    Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
                    Assert.Equal(0, e.NewStartingIndex);
                    Assert.Equal(1, e.NewItems!.Count);
                    Assert.Equal(10, ((IModelIndexableRow)e.NewItems[0]!).ModelIndex);
                    ++raised;
                };

                data.Add(new Row { Id = 10, Caption = "New Row 10" });

                Assert.Equal(11, target.Rows.Count);
                Assert.Equal(1, raised);

                AssertRows(target.Rows, data);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Supports_Removing_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                AssertRows(target.Rows, data);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) =>
                {
                    Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action);
                    Assert.Equal(4, e.OldStartingIndex);
                    Assert.Equal(1, e.OldItems!.Count);
                    Assert.Equal(5, ((IModelIndexableRow)e.OldItems[0]!).ModelIndex);
                    ++raised;
                };

                data.RemoveAt(5);

                Assert.Equal(1, raised);
                AssertRows(target.Rows, data);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Supports_Replacing_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                AssertRows(target.Rows, data);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Remove)
                        Assert.Equal(4, e.OldStartingIndex);
                    else if (e.Action == NotifyCollectionChangedAction.Add)
                        Assert.Equal(0, e.NewStartingIndex);
                    else
                        Assert.True(false, "Unexpected collection change");
                    ++raised;
                };

                data[5] = new Row { Id = 10, Caption = "New Row 10" };

                Assert.Equal(2, raised);
                AssertRows(target.Rows, data);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Supports_Moving_Row()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                AssertRows(target.Rows, data);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Remove)
                        Assert.Equal(4, e.OldStartingIndex);
                    else if (e.Action == NotifyCollectionChangedAction.Add)
                        Assert.Equal(4, e.NewStartingIndex);
                    else
                        Assert.True(false, "Unexpected collection change");
                    ++raised;
                };

                data.Move(5, 8);

                Assert.Equal(2, raised);
                AssertRows(target.Rows, data);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Supports_Clearing_Rows()
            {
                var data = CreateData();
                var target = CreateTarget(data);

                AssertRows(target.Rows, data);

                var raised = 0;
                target.Rows.CollectionChanged += (s, e) =>
                {
                    Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
                    ++raised;
                };

                data.Clear();

                Assert.Equal(1, raised);
                AssertRows(target.Rows, data);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Can_Reassign_Items()
            {
                var data = CreateData();
                var target = CreateTarget(data);
                var raised = 0;

                AssertRows(target.Rows, data);

                target.Rows.CollectionChanged += (s, e) =>
                {
                    Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
                    ++raised;
                };

                target.Items = data = CreateData(20);

                Assert.Equal(1, raised);
                AssertRows(target.Rows, data);
            }

            [AvaloniaFact(Timeout = 10000)]
            public void Raises_Rows_Reset_When_Reassigning_Items_But_Rows_Not_Yet_Read()
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
                ((AnonymousSortableRows<Row>)result.Rows).Sort(new FuncComparer<Row>(
                    new Comparison<Row?>((x, y) => (y?.Id ?? 0) - (x?.Id ?? 0))));
                return result;
            }

            private static void AssertRows(IRows rows, IList<Row> data)
            {
                Assert.Equal(data.Count, rows.Count);

                var sortedData = data.OrderByDescending(x => x.Id).ToList();

                for (var i = 0; i < data.Count; ++i)
                {
                    var row = (IRow<Row>)rows[i];
                    var indexable = (IModelIndexableRow)row;
                    Assert.Same(sortedData[i], row.Model);
                    Assert.Equal(data.IndexOf(row.Model), indexable.ModelIndex);
                }
            }
        }

        public class Selection
        {
            [AvaloniaFact(Timeout = 10000)]
            public void Reassigning_Source_Updates_Selection_Model_Source()
            {
                var data1 = CreateData();
                var data2 = CreateData(5);
                var target = CreateTarget(data1);

                // Ensure selection model is created.
                Assert.Same(data1, ((ITreeDataGridSelection?)target.RowSelection)!.Source);

                target.Items = data2;

                Assert.Same(data2, ((ITreeDataGridSelection?)target.RowSelection)!.Source);
            }
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

        private static void AssertRows(IRows rows, IList<Row> data)
        {
            Assert.Equal(data.Count, rows.Count);

            for (var i = 0; i < data.Count; ++i)
            {
                var row = (IRow<Row>)rows[i];
                var indexable = (IModelIndexableRow)row;
                Assert.Same(row.Model, data[i]);
                Assert.Equal(i, indexable.ModelIndex);
            }
        }

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
