using System;
using Avalonia;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridTextCell : TemplatedControl
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
