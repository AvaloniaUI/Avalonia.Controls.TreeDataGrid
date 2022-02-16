# `TreeDataGrid` Installation

- Add the `Avalonia.Controls.TreeDataGrid` NuGet package to your project
- Add the `TreeDataGrid` theme to your `App.xaml` file (the `StyleInclude` in the following markup):

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="AvaloniaApplication.App">
  <Application.Styles>
    <FluentTheme/>
    <StyleInclude Source="avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml"/>
  </Application.Styles>
</Application>
```
