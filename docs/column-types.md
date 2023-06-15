# TreeDataGrid column types

TreeDataGrid currently supports three different column types: 
- [TextColumn](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/src/Avalonia.Controls.TreeDataGrid/Models/TreeDataGrid/TextColumn.cs) 
- [HierarchicalExpanderColumn](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/src/Avalonia.Controls.TreeDataGrid/Models/TreeDataGrid/HierarchicalExpanderColumn.cs)
- [TemplateColumn](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/src/Avalonia.Controls.TreeDataGrid/Models/TreeDataGrid/TemplateColumn.cs)

## TextColumn
`TextColumn` is useful when you want all cells in the column to have only text values.
Usually, everything you need to instantiate `TextColumn` class is
```csharp
new TextColumn<Person, string>("First Name", x => x.FirstName)
```
The first generic parameter here is your model type basically, the place where you want to grab data from. Person in this case. The second generic parameter here is the type of the property where you want to grab data from. In this case, it is a string, it will be used to know exactly which type your property has.

![image](https://user-images.githubusercontent.com/53405089/157456551-dd394781-903a-4c7b-8874-e631e21534a1.png)

This is the signature of the `TextColumn` constructor. There are two most important parameters. The first one will be used to define the column header, usually, it would be a string. In the sample above it is *"First Name"*. The second parameter is an expression to get the value of the property. In the sample above it is *x => x.FirstName*.

**Note**:               
The sample above is taken from [this article](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/docs/get-started-flat.md). If you feel like you need more examples feel free to check it, there is a sample that shows how to use TextColumns and how to run a whole `TreeDataGrid` using them. 

## CheckBoxColumn

As its name suggests, `CheckBoxColumn` displays a `CheckBox` in its cells. For a readonly checkbox:

```csharp
new CheckColumn<Person>("Firstborn", x => x.IsFirstborn)
```

The first parameter defines the column header. The second parameter is an expression which gets the value of the property from the model.

For a read/write checkbox:

```csharp
new CheckColumn<Person>("Firstborn", x => x.IsFirstborn, (o, v) => o.IsFirstborn = v)
```

This overload adds a second paramter which is the expression used to set the property in the model.

## HierarchicalExpanderColumn
`HierarchicalExpanderColumn` can be used only with `HierarchicalTreeDataGrid` (a.k.a TreeView) thats what Hierarchical stands for in its name, also it can be used only with `HierarchicalTreeDataGridSource`. This type of columns can be useful when you want cells to show an expander to reveal nested data.

That's how you instantiate `HierarchicalExpanderColumn` class:
```csharp
new HierarchicalExpanderColumn<Person>(new TextColumn<Person, string>("First Name", x => x.FirstName), x => x.Children)
```
`HierarchicalExpanderColumn` has only one generic parameter, it is your model type, same as in `TextColumn`, Person in this case.

Lets take a look at the `HierarchicalExpanderColumn` constructor.
![image](https://user-images.githubusercontent.com/53405089/157536079-fd14f1ed-0a7d-438a-abba-fd56766709a9.png)

The first parameter in the constructor is a nested column, you would usually want to display something besides the expander and that's why you need this parameter. In the sample above, we want to display text and we use `TextColumn` for that. The second parameter is a selector of the child elements, stuff that will be displayed when `Expander` is in the expanded state below the parent element.

**Note**:               
The sample above is taken from [this article](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/docs/get-started-hierarchical.md). If you feel like you need more examples feel free to check it, there is a sample that shows how to use `HierarchicalExpanderColumn` and how to run a whole `TreeDataGrid` using it. 

## TemplateColumn

TemplateColumn is the most customizable way to create a column. Because cell contents are described by a data template, the options for how each cell is displayed is almost unlimited.

The `TemplateColumn` constructor takes the following parameters:

- `header`: The Column header
- `cellTemplate`: A data template which describes how cells in the column will be displayed
- `cellEditingTemplate`: A data template which describes how cells in the column will be displayed when in edit mode. If no `cellEditingTemplate` is provided, then edit mode will be disabled for the column.
- `width`: The width of the column
- `options`: Less frequently used options for the column

There are two overloads of the `TemplateColumn` constructor, which corresponds to the two ways in which a template can be specified:

1. Using a `FuncDataTemplate` to create a template in code:

```csharp
new TemplateColumn<Person>(
    "Selected",
    new FuncDataTemplate<Person>((_, _) => new CheckBox
    {
        [!CheckBox.IsCheckedProperty] = new Binding("IsSelected"),
    }))
```

2. Using a template defined as a XAML resource:

```xml
<TreeDataGrid Name="fileViewer" Source="{Binding Files.Source}">
    <TreeDataGrid.Resources>
           
        <!-- Defines a named template for the column -->
        <DataTemplate x:Key="CheckBoxCell">
            <CheckBox IsChecked="{Binding IsSelected}"/>
        </DataTemplate>
        
    </DataTemplate>
              
    </TreeDataGrid.Resources>
</TreeDataGrid>
```

```csharp
// CheckBoxCell is the key of the template defined in XAML.
new TemplateColumn<Person>("Selected", "CheckBoxCell");
```

`TemplateColumn` has only one generic parameter, it is your model type, the same as in `TextColumn`; `Person` in this case. The code above will create a column with header *"Selected"* and a `CheckBox` in each cell.
