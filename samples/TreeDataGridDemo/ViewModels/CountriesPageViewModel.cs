using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using TreeDataGridDemo.Models;

namespace TreeDataGridDemo.ViewModels
{
    internal class CountriesPageViewModel
    {
        private readonly ObservableCollection<Country> _data;

        public CountriesPageViewModel()
        {
            _data = new ObservableCollection<Country>(Countries.All);

            Source = new FlatTreeDataGridSource<Country>(_data)
            {
                Columns =
                {
                    new TextColumn<Country, string>("Country", x => x.Name, (r, v) => r.Name = v, new GridLength(6, GridUnitType.Star)) 
                    { 
                        IsTextSearchEnabled = true 
                    },
                    new TextColumn<Country, string>("Region", x => x.Region, new GridLength(4, GridUnitType.Star)),
                    new TextColumn<Country, int>("Population", x => x.Population, new GridLength(3, GridUnitType.Star)),
                    new TextColumn<Country, int>("Area", x => x.Area, new GridLength(3, GridUnitType.Star)),
                    new TextColumn<Country, int>("GDP", x => x.GDP, new GridLength(3, GridUnitType.Star), new()
                    {
                        MaxWidth = new GridLength(150)
                    }),
                }
            };
            Source.RowSelection!.SingleSelect = false;
        }

        public FlatTreeDataGridSource<Country> Source { get; }

        public void AddCountry(Country country) => _data.Add(country);

        public void RemoveSelected()
        {
            var selection = ((ITreeSelectionModel)Source.Selection!).SelectedIndexes.ToList();

            for (var i = selection.Count - 1; i >= 0; --i)
            {
                _data.RemoveAt(selection[i][0]);
            }
        }
    }
}
