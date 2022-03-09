# TreeDataGrid column types

TreeDataGrid currently supports three different column types: 
- [TextColumn](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/src/Avalonia.Controls.TreeDataGrid/Models/TreeDataGrid/TextColumn.cs) 
- [HierarchicalExpanderColumn](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/src/Avalonia.Controls.TreeDataGrid/Models/TreeDataGrid/HierarchicalExpanderColumn.cs)
- [TemplateColumn](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/src/Avalonia.Controls.TreeDataGrid/Models/TreeDataGrid/TemplateColumn.cs)

## TextColumn
TextColumn is useful when you want all cells in the column to have only text values.
Usually, everything you need to instantiate TextColumn class is
```csharp
new TextColumn<Person, string>("First Name", x => x.FirstName)
```
The first generic parameter here is your model basically, the place where you want to grab data from. Person in this case. The second generic parameter here is the type of the property where you want to grab data from. In this case, it is a string, it will be used to know exactly which type your property has.

![image](https://user-images.githubusercontent.com/53405089/157456551-dd394781-903a-4c7b-8874-e631e21534a1.png)

This is the signature of the TextColumn constructor. There are two most important parameters. The first one will be used to define the column header, usually, it would be a string. In the sample above it is *"First Name"*. The second parameter is an expression to get the value of the property. In the sample above it is *x => x.FirstName*.

**Note**:               
The sample above is taken from [this article](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid/blob/master/docs/get-started-flat.md). If you feel like you need more examples feel free to check it, there is a sample that shows how to use TextColumns and how to run a whole TreeDataGrid using them. 
