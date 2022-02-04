using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Models
{
    public class ColumnListTests
    {
        [Fact]
        public void Columns_Are_Sized_At_End_Of_Measure()
        {
            var target = new ColumnList<Model>
            {
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(100, GridUnitType.Pixel)),
                new TextColumn<Model, string?>(null, x => x.Name, GridLength.Auto),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(1, GridUnitType.Star)),
                new TextColumn<Model, string?>(null, x => x.Name, new GridLength(3, GridUnitType.Star)),
            };

            target.ViewportChanged(new Rect(0, 0, 500, 500));

            for (var row = 0; row < 10; ++row)
            {
                for (var col = 0; col < target.Count; ++col)
                {
                    target.CellMeasured(col, row, new Size(51 + row, 10));
                }
            }

            target.CommitActualWidths();

            Assert.Equal(100, target[0].ActualWidth);
            Assert.Equal(60, target[1].ActualWidth);
            Assert.Equal(85, target[2].ActualWidth);
            Assert.Equal(255, target[3].ActualWidth);
        }

        private class Model
        {
            public string? Name { get; set; }
        }
    }
}
