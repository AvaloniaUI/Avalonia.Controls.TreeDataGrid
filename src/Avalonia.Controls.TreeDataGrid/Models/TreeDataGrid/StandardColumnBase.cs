namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns in an <see cref="ITreeDataGridSource"/> which select cell values from
    /// a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class StandardColumnBase<TModel> : ColumnBase<TModel>
    {
        protected StandardColumnBase(object? header, GridLength width)
            : base(header, width)
        {
        }

        public abstract ICell CreateCell(TModel model);
    }
}
