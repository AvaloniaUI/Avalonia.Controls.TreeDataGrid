using System;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridColumnHeader : Button
    {
        public static readonly StyledProperty<bool> CanUserSortProperty =
            AvaloniaProperty.Register<TreeDataGridColumnHeader, bool>(nameof(CanUserSort), true);

        public static readonly DirectProperty<TreeDataGridColumnHeader, object?> HeaderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, object?>(
                nameof(Header),
                o => o.Header);

        public static readonly DirectProperty<TreeDataGridColumnHeader, ListSortDirection?> SortDirectionProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, ListSortDirection?>(
                nameof(SortDirection),
                o => o.SortDirection);

        private Thumb? _resizer;

        public bool CanUserSort
        {
            get => GetValue(CanUserSortProperty);
            set => SetValue(CanUserSortProperty, value);
        }

        public object? Header => Model?.Header;
        public ListSortDirection? SortDirection => Model?.SortDirection;

        internal IColumn? Model => (IColumn?)DataContext;

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
            if (change.Property == DataContextProperty)
            {
                var oldValue = change.OldValue.GetValueOrDefault<object?>();
                var newValue = change.NewValue.GetValueOrDefault<object?>();
                var oldModel = oldValue as IColumn;
                var newModel = newValue as IColumn;

                if (oldModel is INotifyPropertyChanged oldInpc)
                    oldInpc.PropertyChanged -= ModelPropertyChanged;
                if (newModel is INotifyPropertyChanged newInpc)
                    newInpc.PropertyChanged += ModelPropertyChanged;
                if (!Equals(oldModel?.Header, newModel?.Header))
                    RaisePropertyChanged(HeaderProperty, oldModel?.Header, newModel?.Header);
                if (oldModel?.SortDirection != newModel?.SortDirection)
                    RaisePropertyChanged(SortDirectionProperty, oldModel?.SortDirection, newModel?.SortDirection);
            }

            base.OnPropertyChanged(change);
        }

        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IColumn.SortDirection))
                RaisePropertyChanged(SortDirectionProperty, default, SortDirection);
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
    }
}
