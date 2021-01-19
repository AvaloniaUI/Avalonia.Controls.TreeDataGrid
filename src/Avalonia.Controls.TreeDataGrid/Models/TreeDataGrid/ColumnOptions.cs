using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ColumnOptions<TModel>
    {
        public bool? CanUserResizeColumn { get; set; }
        public bool? CanUserSortColumn { get; set; }
        public Comparison<TModel>? CompareAscending { get; set; }
        public Comparison<TModel>? CompareDescending { get; set; }
    }
}
