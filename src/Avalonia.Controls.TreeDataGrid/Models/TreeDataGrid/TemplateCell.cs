
using System;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TemplateCell : ICell
    {
        public TemplateCell(object? value, Func<Control, IDataTemplate> getCellTemplate)
        {
            GetCellTemplate = getCellTemplate;
            Value = value;
        }

        public bool CanEdit => false;
        public bool SingleTapEdit => false;
        public Func<Control, IDataTemplate> GetCellTemplate { get; }
        public object? Value { get; }
    }
}
