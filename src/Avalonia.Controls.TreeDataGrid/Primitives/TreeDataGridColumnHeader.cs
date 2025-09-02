using System;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;
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

        public static readonly DirectProperty<TreeDataGridColumnHeader, string?> FilterTextProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridColumnHeader, string?>(
                nameof(FilterText),
                o => o.FilterText,
                (o, v) => o.FilterText = v);

        private bool _canUserResize;
        private IColumns? _columns;
        private object? _header;
        private IColumn? _model;
        private ListSortDirection? _sortDirection;
        private bool _showFilter;
        private TreeDataGrid? _owner;
        private Thumb? _resizer;
        private TextBox? _filterBox;
        private string? _filterText;

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

        public bool ShowFilter
        {
            get => _showFilter;
            private set => SetAndRaise(ShowFilterProperty, ref _showFilter, value);
        }

        public string? FilterText
        {
            get => _filterText;
            set 
            {
                if (SetAndRaise(FilterTextProperty, ref _filterText, value))
                {
                    // Update the UI if the filter box exists
                    if (_filterBox != null && _filterBox.Text != value)
                    {
                        _filterBox.Text = value ?? string.Empty;
                    }
                    
                    // Apply the filter
                    SetFilter(value);
                }
            }
        }

        public void Realize(IColumns columns, int columnIndex)
        {
            if (_model is object)
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
            _filterBox = e.NameScope.Find<TextBox>("PART_FilterBox");

            if (_resizer is not null)
            {
                _resizer.DragDelta += ResizerDragDelta;
                _resizer.DoubleTapped += ResizerDoubleTapped;
            }

            if (_filterBox is not null)
            {
                _filterBox.TextChanged += OnFilterTextChanged;
                // If we already have a filter value, apply it to the textbox
                if (!string.IsNullOrEmpty(_filterText))
                {
                    _filterBox.Text = _filterText;
                }
                _filterBox.KeyDown += OnFilterKeyDown;
            }

            // Only update filter if we have a model and owner
            if (_model != null && _owner != null)
            {
                UpdateFilter();
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
            else
            {
                ShowFilter = false;
            }
        }

        private void UpdateFilter()
        {
            var shouldShowFilter = _owner?.ShowColumnFilters == true && HasFilterEnabled();
            ShowFilter = shouldShowFilter;

            if (_filterBox != null && _model != null && shouldShowFilter)
            {
                var currentFilter = GetCurrentFilter();
                _filterText = currentFilter;
                _filterBox.Text = currentFilter ?? string.Empty;
                _filterBox.Watermark = "Filter...";
            }
            else if (_filterBox != null)
            {
                _filterBox.Text = string.Empty;
            }
        }

        private bool HasFilterEnabled()
        {
            if (_model == null)
                return false;
                
            // First check if it implements the new IFilterableColumn interface
            if (IsFilterableColumn(_model))
                return true;
                
            // For backward compatibility, also check the legacy way with TextColumn
            return IsTextColumnWithFilterEnabled(_model);
        }

        private static bool IsFilterableColumn(object column)
        {
            try
            {
                // Check if the column implements IFilterableColumn<T> for any T
                var columnType = column.GetType();
                foreach (var interfaceType in columnType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType && 
                        interfaceType.GetGenericTypeDefinition() == typeof(IFilterableColumn<>))
                    {
                        // Get the IsFilterEnabled property
                        var isFilterEnabledProperty = interfaceType.GetProperty("IsFilterEnabled");
                        return (bool)(isFilterEnabledProperty?.GetValue(column) ?? false);
                    }
                }
            }
            catch
            {
                // If reflection fails, continue to the next check
            }
            
            return false;
        }
        
        private static bool IsTextColumnWithFilterEnabled(object column)
        {
            try
            {
                var type = column.GetType();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(TextColumn<,>))
                {
                    // Use the most specific Options property to avoid ambiguity
                    var optionsProperty = type.GetProperty("Options", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                    if (optionsProperty?.GetValue(column) is object options)
                    {
                        var isFilterEnabledProperty = options.GetType().GetProperty("IsFilterEnabled");
                        return (bool)(isFilterEnabledProperty?.GetValue(options) ?? false);
                    }
                }
            }
            catch
            {
                // If reflection fails, just return false
            }
            return false;
        }

        private string? GetCurrentFilter()
        {
            if (_owner?.Source != null && _model != null)
            {
                var sourceType = _owner.Source.GetType();
                if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(FlatTreeDataGridSource<>))
                {
                    try
                    {
                        var method = sourceType.GetMethod("GetColumnFilter");
                        return method?.Invoke(_owner.Source, new[] { _model }) as string;
                    }
                    catch { }
                }
            }
            return null;
        }

        private void SetFilter(string? filterText)
        {
            if (_owner?.Source != null && _model != null)
            {
                var sourceType = _owner.Source.GetType();
                if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(FlatTreeDataGridSource<>))
                {
                    try
                    {
                        var method = sourceType.GetMethod("SetColumnFilter");
                        method?.Invoke(_owner.Source, new object?[] { _model, filterText });
                    }
                    catch { }
                }
            }
        }

        private void OnFilterTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_filterBox != null)
            {
                // Update the property when the text box changes
                FilterText = _filterBox.Text;
            }
        }

        private void OnFilterKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _filterBox != null)
            {
                _filterBox.Text = string.Empty;
                e.Handled = true;
            }
        }
    }
}
