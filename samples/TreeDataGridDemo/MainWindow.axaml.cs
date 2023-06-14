using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TreeDataGridDemo.Models;
using TreeDataGridDemo.ViewModels;

namespace TreeDataGridDemo
{
    public partial class MainWindow : Window
    {
        private readonly TabControl? _tabs;

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

            Activated += MainWindow_Activated;
        }

        public void AddCountryClick(object sender, RoutedEventArgs e)
        {
            var countries = this.FindControl<TreeDataGrid>("countries");
            var countryTextBox = this.FindControl<TextBox>("countryTextBox");
            var regionTextBox = this.FindControl<TextBox>("regionTextBox");
            var populationTextBox = this.FindControl<TextBox>("populationTextBox");
            var areaTextBox = this.FindControl<TextBox>("areaTextBox");
            var gdpTextBox = this.FindControl<TextBox>("gdpTextBox");

            if (countries is null
                || countryTextBox?.Text is null
                || regionTextBox?.Text is null
                || populationTextBox?.Text is null
                || areaTextBox?.Text is null
                || gdpTextBox?.Text is null)
            {
                return;
            }

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
            var vm = (MainWindowViewModel)DataContext!;
            vm.Countries.AddCountry(country);

            var index = vm.Countries.Source.Rows.Count - 1;
            countries.RowsPresenter!.BringIntoView(index);
            countries.TryGetRow(index)?.Focus();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            Program.Stopwatch!.Stop();
            System.Diagnostics.Debug.WriteLine("Startup time: " + Program.Stopwatch.Elapsed);
        }

        private void SelectedPath_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = (MainWindowViewModel)DataContext!;
                vm.Files.SelectedPath = ((TextBox)sender!).Text;
            }
        }

        private void DragDrop_RowDragStarted(object? sender, TreeDataGridRowDragStartedEventArgs e)
        {
            foreach (DragDropItem i in e.Models)
            {
                if (!i.AllowDrag)
                    e.AllowedEffects = DragDropEffects.None;
            }
        }

        private void DragDrop_RowDragOver(object? sender, TreeDataGridRowDragEventArgs e)
        {
            if (e.Position == TreeDataGridRowDropPosition.Inside &&
                e.TargetRow.Model is DragDropItem i &&
                !i.AllowDrop)
                e.Inner.DragEffects = DragDropEffects.None;
        }

        private void UpdateRealizedCount()
        {
            var tabItem = (TabItem?)_tabs?.SelectedItem;
            if (tabItem is null)
            {
                return;
            }
            var treeDataGrid = (TreeDataGrid?)((Control)tabItem.Content!).GetLogicalDescendants()
                .First(x => x is TreeDataGrid tl);
            var textBlock = (TextBlock)((Control)tabItem.Content!).GetLogicalDescendants()
                .First(x => x is TextBlock tb && tb.Classes.Contains("realized-count"));
            var rows = treeDataGrid!.RowsPresenter!;
            var realizedRowCount = rows.GetRealizedElements().Count();
            var unrealizedRowCount = rows.GetVisualChildren().Count() - realizedRowCount;
            textBlock.Text = $"{realizedRowCount} rows realized ({unrealizedRowCount} unrealized)";
        }
    }
}
