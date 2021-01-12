namespace Avalonia.Controls.Primitives
{
    public abstract class TreeDataGridCell : TemplatedControl, ITreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridCell, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCell, bool>(
                nameof(IsSelected),
                o => o.IsSelected,
                (o, v) => o.IsSelected = v);

        private int _columnIndex = -1;
        private int _rowIndex = -1;
        private bool _isSelected;

        int ITreeDataGridCell.ColumnIndex
        {
            get => _columnIndex;
            set => _columnIndex = value;
        }

        int ITreeDataGridCell.RowIndex
        {
            get => _rowIndex;
            set => _rowIndex = value;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property == IsSelectedProperty)
            {
                PseudoClasses.Set(":selected", change.NewValue.GetValueOrDefault<bool>());
            }
        }
    }
}
