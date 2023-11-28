using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridGruppedColumnHeader : TreeDataGridColumnHeader
    {
        private IColumns? _columns;

        public static readonly DirectProperty<TreeDataGridGruppedColumnHeader, IColumns?> ColumnsProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridGruppedColumnHeader, IColumns?>(
                nameof(Columns),
                o => o.Columns);

        public IColumns? Columns
        {
            get => _columns;
            private set => SetAndRaise(ColumnsProperty, ref _columns, value);
        }

        protected internal override void UpdatePropertiesFromModel(IColumn? model)
        {
            base.UpdatePropertiesFromModel(model);
            if (model is IGruppedColumn columns)
            {
                _columns = columns;
            }
        }
    }
}
