using System;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridExpanderCell : TreeDataGridTextCell
    {
        public static readonly DirectProperty<TreeDataGridExpanderCell, int> IndentProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, int>(
                nameof(Indent),
                o => o.Indent);

        public static readonly DirectProperty<TreeDataGridExpanderCell, bool> IsExpandedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, bool>(
                nameof(IsExpanded),
                o => o.IsExpanded,
                (o, v) => o.IsExpanded = v);

        public static readonly DirectProperty<TreeDataGridExpanderCell, bool> ShowExpanderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridExpanderCell, bool>(
                nameof(ShowExpander),
                o => o.ShowExpander);

        public int Indent => Model?.ModelIndex.GetSize() - 1 ?? 0;

        public bool IsExpanded
        {
            get => Model?.IsExpanded ?? false;
            set
            {
                if (Model is IExpanderCell model)
                    model.IsExpanded = value;
            }
        }

        public bool ShowExpander => Model?.ShowExpander ?? false;

        private IExpanderCell? Model => (IExpanderCell?)DataContext;

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            RaisePropertyChanged(IndentProperty, default, default);
            RaisePropertyChanged(IsExpandedProperty, default, default);
            RaisePropertyChanged(ShowExpanderProperty, default, default);
        }
    }
}
