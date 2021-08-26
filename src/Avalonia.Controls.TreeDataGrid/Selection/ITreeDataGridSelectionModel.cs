namespace Avalonia.Controls.Selection
{
    public interface ITreeDataGridSelectionModel : ITreeSelectionModel
    {
        ISelectionModel RowSelection { get; }
    }
}
