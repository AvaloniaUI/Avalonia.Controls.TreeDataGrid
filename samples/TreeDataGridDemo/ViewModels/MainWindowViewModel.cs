using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

namespace TreeDataGridDemo.ViewModels
{
    internal class MainWindowViewModel
    {
        private CountriesPageViewModel? _countries;
        private FilesPageViewModel? _files;

        public CountriesPageViewModel Countries
        {
            get => _countries ??= new CountriesPageViewModel();
        }

        public FilesPageViewModel Files
        {
            get => _files ??= new FilesPageViewModel();
        }

        public TestPageViewModel Test { get; } = new();
    }

    internal class TestPageViewModel
    {
        public TestPageViewModel()
        {
            Root = new TestNode { Name = "Root" };
            Source = new HierarchicalTreeDataGridSource<TestNode>(Root)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<TestNode>(
                        new TextColumn<TestNode, string>("Name", x => x.Name),
                        x => x.Children),
                },
            };
        }

        public HierarchicalTreeDataGridSource<TestNode> Source { get; }
        public TestNode Root { get; }

        public void DoIt()
        {
            if (Root.Children.Count > 0)
                Root.Children.RemoveAt(0);
            else
                Root.Children.Add(new TestNode { Name = "Child" });
        }
    }

    internal class TestNode
    {
        public string? Name { get; set; }
        public ObservableCollection<TestNode> Children { get; } = new();
    }
}
