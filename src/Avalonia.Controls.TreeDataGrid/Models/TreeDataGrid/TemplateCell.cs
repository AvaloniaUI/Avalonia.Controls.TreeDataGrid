
using System;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TemplateCell : ICell
    {
        public TemplateCell(
            object? value,
            Func<Control, IDataTemplate> getCellTemplate,
            Func<Control, IDataTemplate>? getCellEditingTemplate)
        {
            GetCellTemplate = getCellTemplate;
            GetCellEditingTemplate = getCellEditingTemplate;
            Value = value;
        }

        public bool CanEdit => GetCellEditingTemplate is not null;
        public bool SingleTapEdit => false;
        public Func<Control, IDataTemplate> GetCellTemplate { get; }
        public Func<Control, IDataTemplate>? GetCellEditingTemplate { get; }
        public object? Value { get; }
    }
}
