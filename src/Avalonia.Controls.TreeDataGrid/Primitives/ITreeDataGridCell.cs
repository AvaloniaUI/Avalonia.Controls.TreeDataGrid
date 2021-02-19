namespace Avalonia.Controls.Primitives
{
    internal interface ITreeDataGridCell : IControl, ISelectable
    {
        int ColumnIndex { get; set; }
        int RowIndex { get; set; }
    }
}
