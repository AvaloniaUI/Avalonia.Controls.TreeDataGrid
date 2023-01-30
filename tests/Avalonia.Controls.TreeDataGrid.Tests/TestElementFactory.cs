using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.TreeDataGridTests
{
    internal class TestElementFactory : TreeDataGridElementFactory
    {
        protected override Control CreateElement(object? data)
        {
            return data switch
            {
                LayoutTestCell => new LayoutTestCellControl(),
                _ => base.CreateElement(data),
            };
        }

        protected override string GetDataRecycleKey(object? data)
        {
            return data switch
            {
                LayoutTestCell _ => typeof(LayoutTestCellControl).FullName!,
                _ => base.GetDataRecycleKey(data),
            };
        }
    }
}
