namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns in an <see cref="ITreeDataGridSource"/> which select cell values from
    /// a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class SelectorColumnBase<TModel> : ColumnBase<TModel>
    {
        private GridLength _width;

        protected SelectorColumnBase(object? header, GridLength width)
        {
            Header = header;
            Width = width;
        }

        public override object? Header { get; }

        public override GridLength Width
        {
            get => _width;
            set => _width = value;
        }

        public abstract ICell CreateCell(TModel model);
    }
}
