namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface ITextSearchableColumn<TModel>
    {
        public bool IsTextSearchEnabled { get; set; }
        internal string? SelectValue(TModel model);
    }
}
