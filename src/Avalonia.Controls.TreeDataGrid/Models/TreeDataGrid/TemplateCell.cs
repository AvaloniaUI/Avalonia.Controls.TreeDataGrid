
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TemplateCell : ICell
    {
        public TemplateCell(object? value, IDataTemplate? cellTemplate)
        {
            CellTemplate = cellTemplate;
            Value = value;
        }

        public bool CanEdit => false;
        public IDataTemplate? CellTemplate { get; }
        public object? Value { get; }
    }
}
