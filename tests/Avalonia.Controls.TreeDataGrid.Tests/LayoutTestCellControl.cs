using System.Collections.Generic;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.TreeDataGridTests
{
    internal class LayoutTestCellControl : TreeDataGridTextCell
    {
        public List<Size> MeasureConstraints { get; } = new List<Size>();

        protected override Size MeasureOverride(Size availableSize)
        {
            MeasureConstraints.Add(availableSize);
            return new Size(10, 10);
        }
    }
}
