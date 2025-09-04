# TreeDataGrid Filtering

The TreeDataGrid control includes a powerful and flexible filtering system that allows users to filter data across columns. This document explains how to use the filtering system and customize it for your needs.

## Basic Filtering

TreeDataGrid supports filtering on text columns. To enable filtering:

1. Make sure `ShowColumnFilters` is `true` on the TreeDataGrid (this is the default)
2. Enable filtering for specific columns by setting `IsFilterEnabled` to `true` in the column options:

```csharp
var grid = new TreeDataGrid();

var columns = new ColumnList<Person>(
    new TextColumn<Person, string>(
        "Name", 
        x => x.Name, 
        options: new TextColumnOptions<Person> { IsFilterEnabled = true }
    ),
    new TextColumn<Person, string>(
        "Email", 
        x => x.Email,
        options: new TextColumnOptions<Person> { IsFilterEnabled = true }
    )
);

var source = new FlatTreeDataGridSource<Person>(people) { Columns = columns };
grid.Source = source;
```

This will display filter text boxes below each column header that has filtering enabled. The filter controls will expand to take the full width of the column, ensuring consistent layout and better usability.

**Note:** For standard columns like TextColumn and CheckBoxColumn, setting IsFilterEnabled to true is sufficient. However, for TemplateColumn, you must specify all three properties: FilterValueSelector (to extract the value), Filter (to define how to filter), and FilterControlFactory (to create the filter UI).

## Column Header Filter Layout

The TreeDataGridColumnHeader is implemented as a vertical StackPanel that contains:

1. A header button for displaying the column title and handling sorting
2. A filter control that appears when filtering is enabled

This layout ensures that both the header and filter components take the full width of the column, providing a clean and consistent user interface. The filter control appears directly below the header button when `IsFilterEnabled` is set to `true`.

## Programmatic Access to Filters

You can interact with filters programmatically by accessing the column headers and their filter controls:

```csharp
// Get a reference to the column header
var nameColumnHeader = grid.ColumnHeadersPresenter?
    .TryGetElement(0) as TreeDataGridColumnHeader;

// Access the filter control (if needed)
if (nameColumnHeader != null)
{
    // You can apply filters programmatically through the TreeDataGridSource
    var column = grid.Source.Columns[0];
    grid.Source.SetFilterCondition(column, "Search term");
}
```

## Custom Filter Column Types

The TreeDataGrid supports different types of column filtering through the following systems:

1. **Value Filter System (Recommended)**: Uses the `IValueFilter<TValue>` interface
2. **Legacy System**: Uses the `IFilterableColumn<TModel>` interface

### Using the Value Filter System

The value filter system is the recommended approach for new code. It provides a more flexible and extensible way to define filters:

```csharp
// Define the filter interface
public interface IValueFilter<TValue>
{
    bool Passes(object? condition, TValue? value);
}
```

Every column can use a filter by setting options. The TreeDataGrid provides several built-in filter implementations:

- `TextValueFilter` - For text-based filtering
- `BooleanValueFilter` - For boolean/checkbox filtering
- `SetValueFilter<TValue>` - For filtering based on a set of allowed values

#### Example: Using Text Filter

```csharp
var nameColumn = new TextColumn<Person, string>(
    "Name", 
    x => x.Name,
    options: new TextColumnOptions<Person> { IsFilterEnabled = true }
);
// Filter is automatically created when IsFilterEnabled is true

// Or set a filter manually in the options:
var options = new TextColumnOptions<Person> { 
    IsFilterEnabled = true,
    Filter = new TextValueFilter(TextFilterMode.Contains, false) 
};
```

#### Example: Using Template Column with Filter

```csharp
var regionColumn = new TemplateColumn<Country>(
    "Region", 
    "RegionCellTemplate",  // Template resource key
    "RegionEditCellTemplate",  // Edit template resource key
    options: new TemplateColumnOptions<Country> 
    { 
        // While IsFilterEnabled is used for TextColumn, it's not required for TemplateColumn when all three properties below are specified
        // FilterValueSelector is required to extract the value to filter on
        FilterValueSelector = x => x.Region,
        // Filter specifies how to filter the values
        Filter = new TextValueFilter(),
        // FilterControlFactory is required to create the filter UI
        FilterControlFactory = (col) => new TextFilterControl(col, "custom filter...")
    }
);
// For TemplateColumn, all three properties above should be specified
```


#### Example: Using Boolean Filter

```csharp
var activeColumn = new CheckBoxColumn<User>(
    "Active", 
    x => x.IsActive,
    options: new CheckBoxColumnOptions<User> { IsFilterEnabled = true }
);
// The BooleanValueFilter is automatically created when IsFilterEnabled is true
```


### The `TextValueFilter` supports different filter modes:
- `Contains` - Checks if the value contains the filter text (default)
- `StartsWith` - Checks if the value starts with the filter text
- `EndsWith` - Checks if the value ends with the filter text

You can configure these modes when creating a filter:

### Creating Custom Filter Columns

Columns implement the `IFilterableColumn<TModel>` interface to provide filtering:

```csharp
public interface IFilterableColumn
{
    /// <summary>
    /// Gets a value indicating whether filtering is enabled for this column.
    /// </summary>
    bool IsFilterEnabled { get; }
}

public interface IFilterableColumn<TModel>: IFilterableColumn
{
    /// <summary>
    /// Determines if the model passes the filter.
    /// </summary>
    /// <param name="model">The model to check.</param>
    /// <param name="condition">The filter condition to apply.</param>
    /// <returns>True if the model passes the filter, otherwise false.</returns>
    bool PassesFilter(TModel model, object? condition);
}
```


### Performance Considerations

For large datasets, consider these performance tips:

1. If possible, filter data at the source (database query) rather than in-memory
2. Implement debounced filtering (apply filter after typing stops)
3. Keep filter implementations efficient
4. Consider caching filter results when appropriate

## Complete Example

See the `CustomFilteringSample.cs` in the samples directory for a complete example of implementing custom filtering UI with TreeDataGrid.