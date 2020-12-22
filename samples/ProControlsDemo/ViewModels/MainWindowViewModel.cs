using System.Collections.ObjectModel;
using Avalonia.Controls;
using ProControlsDemo.Models;

namespace ProControlsDemo.ViewModels
{
    internal class MainWindowViewModel
    {
        private HierarchicalTreeDataGridSource<TreeNodeModel>? _files;
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

        public HierarchicalTreeDataGridSource<TreeNodeModel> Files
        {
            get
            {
                if (_files is null)
                {
                    var model = new TreeNodeModel(@"c:\", isDirectory: true, isRoot: true);
                    var source = new HierarchicalTreeDataGridSource<TreeNodeModel>(model, x => x.Children, x => x.IsDirectory);
                    source.AddColumn("Name", x => x.Name);
                    source.AddColumn("Size", x => x.Size, GridLength.Auto);
                    source.AddColumn("Modified", x => x.Modified, GridLength.Auto);
                    _files = source;
                }

                return _files;
            }
        }

        public void AddCountry(Country country) => _countryData?.Add(country);
    }
}
