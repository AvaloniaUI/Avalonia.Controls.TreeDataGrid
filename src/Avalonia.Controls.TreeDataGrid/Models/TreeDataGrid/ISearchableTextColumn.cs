namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface ITextSearchableColumn<TModel>
    {
        bool IsTextSearchEnabled { get; }
        string? SelectValue(TModel model);
    }
}
