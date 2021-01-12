namespace Avalonia.Controls.Primitives
{
    public interface ITreeDataGridCell : IControl, ISelectable
    {
        int ColumnIndex { get; set; }
        int RowIndex { get; set; }
    }
}
