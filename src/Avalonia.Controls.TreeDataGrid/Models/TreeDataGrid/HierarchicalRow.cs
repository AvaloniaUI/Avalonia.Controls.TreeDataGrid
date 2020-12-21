namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class HierarchicalRow<TModel> : RowBase<TModel>
    {
        public HierarchicalRow(TModel model, IndexPath modelIndex)
        {
            Model = model;
            ModelIndex = modelIndex;
        }

        public override object? Header => null;

        public override GridLength Height
        {
            get => GridLength.Auto;
            set { }
        }

        public override TModel Model { get; }
        public IndexPath ModelIndex { get; }
    }
}
