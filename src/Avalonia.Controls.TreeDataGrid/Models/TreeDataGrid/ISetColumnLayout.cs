namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Internal low-level interface for layout interactions between <see cref="IColumns"/> and
    /// <see cref="IColumn"/>.
    /// </summary>
    public interface ISetColumnLayout
    {
        void SetActualWidth(double width);
        void SetWidth(GridLength width);
    }
}
