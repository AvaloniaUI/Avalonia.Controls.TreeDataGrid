namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns in an <see cref="ITreeDataGridSource"/> which select cell values from
    /// a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class SelectorColumnBase<TModel> : ColumnBase<TModel>
    {
        protected SelectorColumnBase(object? header, GridLength width)
            : base(header, width)
        {
        }

        public abstract ICell CreateCell(TModel model);
    }
}
