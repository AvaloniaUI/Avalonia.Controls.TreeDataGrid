using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Utilities;

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

        public static readonly DirectProperty<TreeDataGridColumnHeader, bool> ShowFilterProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, bool>(
                nameof(ShowFilter),
                o => o.ShowFilter);
        
        private bool _canUserResize;
        private IColumns? _columns;
        private object? _header;
        private IColumn? _model;
        private ListSortDirection? _sortDirection;
        private TreeDataGrid? _owner;
        private Thumb? _resizer;
        private IFilterControl? _filterControl;

        public bool CanUserResize
        {
            get => _canUserResize;
            private set => SetAndRaise(CanUserResizeProperty, ref _canUserResize, value);
        }

        public int ColumnIndex { get; private set; }

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
        
        public bool ShowFilter => _model is IFilterableColumn filterable && filterable.IsFilterEnabled;


        public void Realize(IColumns columns, int columnIndex)
        {
            if (_model != null)
                throw new InvalidOperationException("Column header is already realized.");

            _columns = columns;
            _model = columns[columnIndex];
            ColumnIndex = columnIndex;
            UpdatePropertiesFromModel();

            if (_model is INotifyPropertyChanged newInpc)
                newInpc.PropertyChanged += OnModelPropertyChanged;
        }

        public void UpdateColumnIndex(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }

        public void Unrealize()
        {
            if (_model is INotifyPropertyChanged oldInpc)
                oldInpc.PropertyChanged -= OnModelPropertyChanged;

            _columns = null;
            _model = null;
            ColumnIndex = -1;
            UpdatePropertiesFromModel();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _resizer = e.NameScope.Find<Thumb>("PART_Resizer");
            var filterContainer = e.NameScope.Find<ContentControl>("PART_FilterBox");

            if (_resizer is not null)
            {
                _resizer.DragDelta += ResizerDragDelta;
                _resizer.DoubleTapped += ResizerDoubleTapped;
            }

            // Only update filter if we have a model and owner
            if (_model != null && _owner != null)
            {
                UpdateFilter();
            }

            if (filterContainer != null)
            {
                filterContainer.IsVisible = ShowFilter;
                if (_filterControl != null)
                {
                    filterContainer.Content = _filterControl.Control;
                }
            }
        }

        private void ResizerDoubleTapped(object? sender, Interactivity.RoutedEventArgs e)
        {
            _columns?.SetColumnWidth(ColumnIndex, GridLength.Auto);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);

            // HACKFIX for #83. Seems that cells are getting truncated at times due to DPI scaling.
            // New text stack in Avalonia 11.0 should fix this but until then a hack to add a pixel
            // to cell size seems to fix it.
            result = result.Inflate(new Thickness(1, 0));

            return result;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == CanUserResizeProperty)
            {
                PseudoClasses.Set(":resizable", change.GetNewValue<bool>());
            }
            else if (change.Property == DataContextProperty)
            {
                var oldModel = change.GetOldValue<object?>() as IColumn;
                var newModel = change.GetNewValue<object?>() as IColumn;

                if (oldModel is INotifyPropertyChanged oldInpc)
                    oldInpc.PropertyChanged -= OnModelPropertyChanged;
                if (newModel is INotifyPropertyChanged newInpc)
                    newInpc.PropertyChanged += OnModelPropertyChanged;

                UpdatePropertiesFromModel();
            }
            else if (change.Property == ParentProperty)
            {
                if (_owner is not null)
                    _owner.PropertyChanged -= OnOwnerPropertyChanged;
                _owner = change.GetNewValue<StyledElement>()?.TemplatedParent as TreeDataGrid;
                if (_owner is not null)
                    _owner.PropertyChanged += OnOwnerPropertyChanged;
                UpdatePropertiesFromModel();
            }

            base.OnPropertyChanged(change);
        }

        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IColumn.CanUserResize) ||
                e.PropertyName == nameof(IColumn.Header) ||
                e.PropertyName == nameof(IColumn.SortDirection))
                UpdatePropertiesFromModel();
        }

        private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_owner is null)
                return;
            if (e.Property == TreeDataGrid.CanUserResizeColumnsProperty)
                CanUserResize = _model?.CanUserResize ?? _owner.CanUserResizeColumns;
        }

        private void ResizerDragDelta(object? sender, VectorEventArgs e)
        {
            if (_columns is null || _model is null || MathUtilities.IsZero(e.Vector.X))
                return;

            var pixelWidth = _model.Width.IsAbsolute ? _model.Width.Value : Bounds.Width;

            if (double.IsNaN(pixelWidth) || double.IsInfinity(pixelWidth) || pixelWidth + e.Vector.X < 0)
                return;

            var width = new GridLength(pixelWidth + e.Vector.X, GridUnitType.Pixel);
            _columns.SetColumnWidth(ColumnIndex, width);
        }

        private void UpdatePropertiesFromModel()
        {
            CanUserResize = _model?.CanUserResize ?? _owner?.CanUserResizeColumns ?? false;
            Header = _model?.Header;
            SortDirection = _model?.SortDirection;

            // Only update filter if we have all necessary references
            if (_model != null && _owner != null)
            {
                UpdateFilter();
            }
        }

        private void UpdateFilter()
        {
            // Find the ContentControl named PART_FilterBox
            var contentControl = this.GetTemplateChildren().OfType<ContentControl>()
                .FirstOrDefault(x => x.Name == "PART_FilterBox");
            // Clean up any existing filter control
            if (_filterControl != null)
            {
                _filterControl.FilterValueChanged -= OnFilterValueChanged;

                // Remove from visual tree if it was added

                if (contentControl != null)
                {
                    contentControl.Content = null;
                }

                _filterControl = null;
            }

            if (_model == null) return;
            var options = _model.ErasedOptions();

            // Get the filter control factory from the column options
            if (options is not IFilterControlFactory factory) return;

            // Create a filter control using the factory
            _filterControl = factory.CreateFilterControl(_model, null);

            if (_filterControl == null) return;

            // Subscribe to filter value changes
            _filterControl.FilterValueChanged += OnFilterValueChanged;

            // Add to visual tree
            var control = _filterControl.Control;
            
            if (contentControl != null)
            {
                contentControl.Content = control;
            }
        }

        private Dictionary<IFilterableColumn, object?> _filterConditions = new();

        private void OnFilterValueChanged(object? sender, FilterValueChangedEventArgs e)
        {
            // Update the filter value
            if (_owner?.Source == null || _model == null) return;
            _filterConditions[e.Column] = e.FilterCondition;
            
        }
    }
}
