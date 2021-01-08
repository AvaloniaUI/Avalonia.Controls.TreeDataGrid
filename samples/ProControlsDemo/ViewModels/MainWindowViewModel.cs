using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ProControlsDemo.Models;

namespace ProControlsDemo.ViewModels
{
    internal class MainWindowViewModel
    {
        private HierarchicalTreeDataGridSource<FileTreeNodeModel>? _files;
        private ObservableCollection<Country>? _countryData;
        private FlatTreeDataGridSource<Country>? _countries;

        public FlatTreeDataGridSource<Country> Countries
        {
            get
            {
                if (_countries is null)
                {
                    _countryData ??= new ObservableCollection<Country>(Models.Countries.All);
                    _countries = new FlatTreeDataGridSource<Country>(_countryData);
                    _countries.AddColumn("Country", x => x.Name, new GridLength(6, GridUnitType.Star));
                    _countries.AddColumn("Region", x => x.Region, new GridLength(4, GridUnitType.Star));
                    _countries.AddColumn("Popuplation", x => x.Population, new GridLength(3, GridUnitType.Star));
                    _countries.AddColumn("Area", x => x.Area, new GridLength(3, GridUnitType.Star));
                    _countries.AddColumn("GDP", x => x.GDP, new GridLength(3, GridUnitType.Star));
                }

                return _countries;
            }
        }

        public HierarchicalTreeDataGridSource<FileTreeNodeModel> Files
        {
            get
            {
                if (_files is null)
                {
                    var model = new FileTreeNodeModel(@"c:\", isDirectory: true, isRoot: true);
                    var source = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(model);
                    source.AddExpanderColumn(
                        "Name",
                        x => x.Name,
                        x => x.Children,
                        x => x.IsDirectory,
                        new GridLength(1, GridUnitType.Star),
                        new ColumnOptions<FileTreeNodeModel>
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                        });
                    source.AddColumn(
                        "Size",
                        x => x.Size,
                        GridLength.Auto,
                        new ColumnOptions<FileTreeNodeModel>
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                        });
                    source.AddColumn(
                        "Modified",
                        x => x.Modified,
                        GridLength.Auto,
                        new ColumnOptions<FileTreeNodeModel>
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Modified),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Modified),
                        });

                    _files = source;
                }

                return _files;
            }
        }

        public void AddCountry(Country country) => _countryData?.Add(country);
    }
}
