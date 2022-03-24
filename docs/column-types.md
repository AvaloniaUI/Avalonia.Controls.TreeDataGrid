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
TemplateColumn is the most customizable option to create a column. You can put basically everything that you can put into `IDataTemplate` into this column cells.

That's how you instantiate `TemplateColumn` class:
```csharp
new TemplateColumn<Person>(
                        "Selected",
                        new FuncDataTemplate<FileTreeNodeModel>((a,e) => new CheckBox()))
```
`TemplateColumn` has only one generic parameter, it is your model type, same as in `TextColumn`, Person in this case. Code above will create a column with header *"Selected"* and `CheckBox` in each cell.

![image](https://user-images.githubusercontent.com/53405089/157664231-8653bce9-f8d6-4fbc-8e78-e3ff93f1ace2.png)

`TemplateColumn` has only two required parameters. The first one is the column header as everywhere. The second is `IDataTemplate` basically, a template that contains stuff that you want to be displayed in the cells of this column.
