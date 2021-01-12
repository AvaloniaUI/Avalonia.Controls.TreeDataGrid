using System;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridTextCell : TreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridTextCell, string?> ValueProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, string?>(
                nameof(Value),
                o => o.Value);

        public string? Value => ((ICell?)DataContext)?.Value?.ToString();

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            RaisePropertyChanged(ValueProperty, default, default);
        }
    }
}
