using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
