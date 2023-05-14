# Selection

Two selection modes are supported:

- Row selection allows the user to select whole rows
- Cell selection allows the user to select individial cells

Both selection types support either single or multiple selection. The default selection type is single row selection.

## Index Paths

Because `TreeDataGrid` supports hierarchical data, using a simple index to identify a row in the data source isn't enough. Instead indexes are represented using the `IndexPath` struct.

An `IndexPath` is essentially an array of indexes, each element of which specifies the index at a succesively deeper level in the hierarchy of the data.

Consider the following data source:

```
|- A
|  |- B
|  |- C
|     |- D
|- E
```

- `A` has an index path of `0` as it's the first item at the root of the hierarchy
- `B` has an index path of `0,0` as it's the first child of the first item
- `C` has an index path of `0,1` as it's the second child of the first item
- `D` has an index path of `0,1,0` as it's the first child of `C`
- `E` has an index path of `1` as it's the second item in the root

`IndexPath` is an immutable struct which is constructed with an array of integers, e.g.: `new ItemPath(0, 1, 0)`. There is also an implicit conversion from `int` for when working with a flat data source.

## Row Selection

Row selection is the default and is exposed via the `RowSelection` property on the `FlatTreeDataGridSource<TModel>` and `HierarchicalTreeDataGridSource<TModel>` classes when enabled. Row selection is stored in an instance of the `TreeDataGridRowSelectionModel<TModel>` class.

By default is single selection. To enable multiple selection set the the `SingleSelect` property to `false`, e.g.:

```csharp
Source = new FlatTreeDataGridSource<Person>(_people)
{
    Columns =
    {
        new TextColumn<Person, string>("First Name", x => x.FirstName),
        new TextColumn<Person, string>("Last Name", x => x.LastName),
        new TextColumn<Person, int>("Age", x => x.Age),
    },
};

Source.RowSelection!.SingleSelect = false;
```

The properties on `ITreeDataGridRowSelectionModel<TModel>` can be used to manipulate the selection, e.g.:

```csharp
Source.RowSelection!.SelectedIndex = 1;
```

Or

```csharp
Source.RowSelection!.SelectedIndex = new IndexPath(0, 1);
```

## Cell Selection

To enable cell selection for a `TreeDataGridSource`, assign an instance of `TreeDataGridCellSelectionModel<TModel>` to the source's `Selection` property:

```csharp
Source = new FlatTreeDataGridSource<Person>(_people)
{
    Columns =
    {
        new TextColumn<Person, string>("First Name", x => x.FirstName),
        new TextColumn<Person, string>("Last Name", x => x.LastName),
        new TextColumn<Person, int>("Age", x => x.Age),
    },
};

Source.Selection = new TreeDataGridCellSelectionModel<Person>(Source);
```

Or for multiple cell selection:

```csharp
Source.Selection = new TreeDataGridCellSelectionModel<Person>(Source) { SingleSelect = false };
```

Cell selection is is exposed via the `CellSelection` property on the `FlatTreeDataGridSource<TModel>` and `HierarchicalTreeDataGridSource<TModel>` classes when enabled.

The `CellIndex` struct indentifies an individual cell with by combination of an integer column index and an `IndexPath` row index.