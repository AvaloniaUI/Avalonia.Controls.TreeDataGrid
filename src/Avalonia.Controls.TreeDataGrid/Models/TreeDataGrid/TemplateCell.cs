
using System;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TemplateCell : ICell
    {
        public TemplateCell(object? value, Func<IControl, IDataTemplate> getCellTemplate)
        {
            GetCellTemplate = getCellTemplate;
            Value = value;
        }

        public bool CanEdit => false;
        public Func<IControl, IDataTemplate> GetCellTemplate { get; }
        public object? Value { get; }
    }
}
