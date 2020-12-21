namespace Avalonia.Controls.Primitives
{
    public abstract class TreeDataGridCell : TemplatedControl, ISelectable
    {
        public static readonly DirectProperty<TreeDataGridCell, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridCell, bool>(
                nameof(IsSelected),
                o => o.IsSelected,
                (o, v) => o.IsSelected = v);

        private bool _isSelected;

        public int ColumnIndex { get; internal set; }
        public int RowIndex { get; internal set; }
        
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
