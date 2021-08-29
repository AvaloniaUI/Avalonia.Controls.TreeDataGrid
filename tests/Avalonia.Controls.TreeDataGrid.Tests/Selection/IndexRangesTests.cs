using Avalonia.Controls.Selection;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Selection
{
    public class IndexRangesTests
    {
        [Fact]
        public void RemoveRange_Removes_Indexes_Intersecting_Start()
        {
            var target = CreateTarget();

            target.RemoveRange(new IndexPath(1), new IndexPath(1, 2, 2));

            Assert.Single(target, new IndexPath(1, 2, 3));
        }

        [Fact]
        public void RemoveRange_Removes_Indexes_Intersecting_End()
        {
            var target = CreateTarget();

            target.RemoveRange(new IndexPath(1, 1, 1), new IndexPath(2));

            Assert.Equal(2, target.Count);
            Assert.Equal(new IndexPath(1, 1), target[0]);
            Assert.Equal(new IndexPath(1, 1, 0), target[1]);
        }

        private static IndexRanges CreateTarget()
        {
            var result = new IndexRanges();
            result.Add(new IndexPath(1, 1));
            result.Add(new IndexPath(1, 1, 0));
            result.Add(new IndexPath(1, 1, 2));
            result.Add(new IndexPath(1, 2));
            result.Add(new IndexPath(1, 2, 0));
            result.Add(new IndexPath(1, 2, 1));
            result.Add(new IndexPath(1, 2, 2));
            result.Add(new IndexPath(1, 2, 3));
            return result;
        }
    }
}
