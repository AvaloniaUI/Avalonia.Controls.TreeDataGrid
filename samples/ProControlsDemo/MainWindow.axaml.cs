using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
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
        private readonly TreeDataGrid _fileViewer;
        private readonly TabControl _tabs;
        private HashSet<FileTreeNodeModel>? _fileSelection;

        public MainWindow()
        {
            InitializeComponent();
            this.AttachDevTools();
            Renderer.DrawFps = true;
            DataContext = new MainWindowViewModel();

            _tabs = this.FindControl<TabControl>("tabs");
            _fileViewer = this.FindControl<TreeDataGrid>("fileViewer");
            _fileViewer.BeforeSort += SaveFileSelection;
            _fileViewer.AfterSort += RestoreFileSelection;

            DispatcherTimer.Run(() =>
            {
                UpdateRealizedCount();
                return true;
            }, TimeSpan.FromMilliseconds(500));

            Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            Program.Stopwatch!.Stop();
            System.Diagnostics.Debug.WriteLine("Startup time: " + Program.Stopwatch.Elapsed);
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

        private void UpdateRealizedCount()
        {
            var tabItem = (TabItem)_tabs.SelectedItem!;
            var treeDataGrid = (TreeDataGrid)((Control)tabItem.Content).GetLogicalDescendants()
                .FirstOrDefault(x => x is TreeDataGrid tl);
            var textBlock = (TextBlock)((Control)tabItem.Content).GetLogicalDescendants()
                .FirstOrDefault(x => x is TextBlock tb && tb.Classes.Contains("realized-count"));
            var rows = treeDataGrid.RowsPresenter!;
            var realizedRowCount = rows.RealizedElements.Count;
            var unrealizedRowCount = ((ILogical)rows).LogicalChildren.Count - realizedRowCount;
            textBlock.Text = $"{realizedRowCount} rows realized ({unrealizedRowCount} unrealized)";
        }

        private void SaveFileSelection(object? sender, EventArgs e)
        {
            var selection = (HierarchicalSelectionModel<FileTreeNodeModel>)_fileViewer.Selection!;
            _fileSelection = new HashSet<FileTreeNodeModel>(selection.SelectedItems);
        }

        private void RestoreFileSelection(object? sender, EventArgs e)
        {
            if (_fileSelection?.Count > 0)
            {
                var rows = _fileViewer.Source!.Rows;
                var selection = _fileViewer.Selection!;
                selection.BeginBatchUpdate();

                for (var i = 0; i < rows.Count; ++i)
                {
                    var row = (IRow<FileTreeNodeModel>)rows[i];

                    if (_fileSelection.Contains(row.Model))
                    {
                        selection.Select(i);
                    }
                }

                selection.EndBatchUpdate();
            }
        }
    }
}
