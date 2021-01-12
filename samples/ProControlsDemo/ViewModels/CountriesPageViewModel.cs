using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using ProControlsDemo.Models;

namespace ProControlsDemo.ViewModels
{
    internal class CountriesPageViewModel
    {
        private ObservableCollection<Country> _data;

        public CountriesPageViewModel()
        {
            _data = new ObservableCollection<Country>(Countries.All);

            Source = new FlatTreeDataGridSource<Country>(_data)
            {
                Columns =
                {
                    new TextColumn<Country, string>("Country", x => x.Name, new GridLength(6, GridUnitType.Star)),
                    new TextColumn<Country, string>("Region", x => x.Region, new GridLength(4, GridUnitType.Star)),
                    new TextColumn<Country, int>("Population", x => x.Population, new GridLength(3, GridUnitType.Star)),
                    new TextColumn<Country, int>("Area", x => x.Area, new GridLength(3, GridUnitType.Star)),
                    new TextColumn<Country, int>("GDP", x => x.GDP, new GridLength(3, GridUnitType.Star)),
                }
            };

            Selection = new SelectionModel<IRow>(Source.Rows);
        }

        public FlatTreeDataGridSource<Country> Source { get; }
        public SelectionModel<IRow> Selection { get; }

        public void AddCountry(Country country) => _data.Add(country);
    }
}
