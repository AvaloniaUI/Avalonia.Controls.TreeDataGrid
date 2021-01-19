using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridColumnHeader : Button
    {
        public static readonly DirectProperty<TreeDataGridColumnHeader, bool> CanUserResizeProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, bool>(
                nameof(CanUserResize),
                x => x.CanUserResize);

        public static readonly DirectProperty<TreeDataGridColumnHeader, object?> HeaderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, object?>(
                nameof(Header),
                o => o.Header);

        public static readonly DirectProperty<TreeDataGridColumnHeader, ListSortDirection?> SortDirectionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, ListSortDirection?>(
                nameof(SortDirection),
                o => o.SortDirection);

        private bool _canUserResize;
        private object? _header;
        private ListSortDirection? _sortDirection;
        private TreeDataGrid? _owner;
        private Thumb? _resizer;

        public bool CanUserResize
        {
            get => _canUserResize;
            private set => SetAndRaise(CanUserResizeProperty, ref _canUserResize, value);
        }

        public object? Header
        {
            get => _header;
            private set => SetAndRaise(HeaderProperty, ref _header, value);
        }

        public ListSortDirection? SortDirection
        {
            get => _sortDirection;
            private set => SetAndRaise(SortDirectionProperty, ref _sortDirection, value);
        }

        private IColumn? Model => (IColumn?)DataContext;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _resizer = e.NameScope.Find<Thumb>("PART_Resizer");

            if (_resizer is object)
            {
                _resizer.DragDelta += ResizerDragDelta;
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property == CanUserResizeProperty)
            {
                PseudoClasses.Set(":resizable", change.NewValue.GetValueOrDefault<bool>());
            }
            else if (change.Property == DataContextProperty)
            {
                var oldModel = change.OldValue.GetValueOrDefault<object?>() as IColumn;
                var newModel = change.NewValue.GetValueOrDefault<object?>() as IColumn;

                if (oldModel is INotifyPropertyChanged oldInpc)
                    oldInpc.PropertyChanged -= OnModelPropertyChanged;
                if (newModel is INotifyPropertyChanged newInpc)
                    newInpc.PropertyChanged += OnModelPropertyChanged;

                UpdatePropertiesFromModel();
            }
            else if (change.Property == ParentProperty)
            {
                if (_owner is object)
                    _owner.PropertyChanged -= OnOwnerPropertyChanged;
                _owner = change.NewValue.GetValueOrDefault<IControl>()?.TemplatedParent as TreeDataGrid;
                if (_owner is object)
                    _owner.PropertyChanged += OnOwnerPropertyChanged;
                UpdatePropertiesFromModel();
            }

            base.OnPropertyChanged(change);
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IColumn.CanUserResize) ||
                e.PropertyName == nameof(IColumn.Header) ||
                e.PropertyName == nameof(IColumn.SortDirection))
                UpdatePropertiesFromModel();
        }

        private void OnOwnerPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_owner is null)
                return;
            if (e.Property == TreeDataGrid.CanUserResizeColumnsProperty)
                CanUserResize = Model?.CanUserResize ?? _owner.CanUserResizeColumns;
        }

        private void ResizerDragDelta(object? sender, VectorEventArgs e)
        {
            if (Model is null || MathUtilities.IsZero(e.Vector.X))
                return;

            var width = Model.Width.IsAbsolute ? Model.Width.Value : Bounds.Width;

            if (double.IsNaN(width) || double.IsInfinity(width) || width + e.Vector.X < 0)
                return;

            Model.Width = new GridLength(width + e.Vector.X, GridUnitType.Pixel);

            this.FindAncestorOfType<TreeDataGridPanel>()?.InvalidateAll();
        }

        private void UpdatePropertiesFromModel()
        {
            var model = Model;
            CanUserResize = model?.CanUserResize ?? _owner?.CanUserResizeColumns ?? false;
            Header = model?.Header;
            SortDirection = model?.SortDirection;
        }
    }
}
