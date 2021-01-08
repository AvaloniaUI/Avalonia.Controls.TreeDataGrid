using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ColumnOptions<TModel>
    {
        private Comparison<TModel>? _compareAscending;
        private Comparison<TModel>? _compareDescending;

        public bool UseDefaultComparison { get; set; }

        public Comparison<TModel>? CompareAscending 
        {
            get => _compareAscending;
            set
            {
                _compareAscending = value;
                UseDefaultComparison = false;
            }
        }

        public Comparison<TModel>? CompareDescending
        {
            get => _compareDescending;
            set
            {
                _compareDescending = value;
                UseDefaultComparison = false;
            }
        }
    }
}
