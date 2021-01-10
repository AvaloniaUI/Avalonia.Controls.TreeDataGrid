using System;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridTemplateCell : ContentControl
    {
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            var cell = DataContext as TemplateCell;
            Content = cell?.Value;
            ContentTemplate = cell?.CellTemplate;
        }
    }
}
