using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ProControlsDemo.Models;
using ProControlsDemo.ViewModels;

namespace ProControlsDemo
{
    public class MainWindow : Window
    {
        private readonly TabControl _tabs;

        public MainWindow()
        {
            InitializeComponent();
            this.AttachDevTools();
            DataContext = new MainWindowViewModel();

            _tabs = this.FindControl<TabControl>("tabs");

            DispatcherTimer.Run(() =>
            {
                UpdateRealizedCount();
                return true;
            }, TimeSpan.FromMilliseconds(500));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void AddCountryClick(object sender, RoutedEventArgs e)
        {
            var countryTextBox = this.FindControl<TextBox>("countryTextBox");
            var regionTextBox = this.FindControl<TextBox>("regionTextBox");
            var populationTextBox = this.FindControl<TextBox>("populationTextBox");
            var areaTextBox = this.FindControl<TextBox>("areaTextBox");
            var gdpTextBox = this.FindControl<TextBox>("gdpTextBox");

            var country = new Country(
                countryTextBox.Text,
                regionTextBox.Text,
                int.TryParse(populationTextBox.Text, out var population) ? population : 0,
                int.TryParse(areaTextBox.Text, out var area) ? area : 0,
                0,
                0,
                null,
                null,
                int.TryParse(gdpTextBox.Text, out var gdp) ? gdp : 0,
                null,
                null,
                null,
                null);
            ((MainWindowViewModel)DataContext!).Countries.AddCountry(country);
        }

        private void FilesPointerPressed(object sender, PointerPressedEventArgs e)
        {
            // We're only interested in right clicks.
            if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                return;

            // Sender will contain the TreeDataGrid control.
            var treeDataGrid = (TreeDataGrid)sender;
            
            // e.Source will contain the clicked control, we can use TryGetRowModel to return
            // the model for the row that control is part of.
            if (e.Source is Control source &&
                treeDataGrid.TryGetRowModel<FileTreeNodeModel>(source, out var row))
            {
                // Create the context menu.
                var contextMenu = new ContextMenu();

                // And customize according to its contents.
                if (row.IsDirectory)
                {
                    contextMenu.Items = new[] 
                    { 
                        new MenuItem { Header = "Open Directory" } 
                    };
                }
                else
                {
                    contextMenu.Items = new[]
                    {
                        new MenuItem { Header = "Open File" }
                    };
                }

                // Show the menu.
                contextMenu.Open(source);
            }
        }

        private void UpdateRealizedCount()
        {
            var tabItem = (TabItem)_tabs.SelectedItem!;
            var treeDataGrid = (TreeDataGrid)((Control)tabItem.Content).GetLogicalDescendants()
                .FirstOrDefault(x => x is TreeDataGrid tl);
            var textBlock = (TextBlock)((Control)tabItem.Content).GetLogicalDescendants()
                .FirstOrDefault(x => x is TextBlock tb && tb.Classes.Contains("realized-count"));
            var repeater = (IVisual)treeDataGrid.Repeater!;
            var rows = repeater!.VisualChildren.Count / treeDataGrid.Columns?.Count;
            textBlock.Text = $"{rows} rows realized";
        }
    }
}
