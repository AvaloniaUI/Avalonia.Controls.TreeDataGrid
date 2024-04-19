using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using ReactiveUI;
using TreeDataGridDemo.Models;

namespace TreeDataGridDemo.ViewModels
{
    internal class CountriesPageViewModel : ReactiveObject
    {
        private readonly ObservableCollection<Country> _data;
        private bool _cellSelection;

        public CountriesPageViewModel()
        {
            _data = new ObservableCollection<Country>(Countries.All);

            Source = new FlatTreeDataGridSource<Country>(_data)
            {
                Columns =
                {
                    new TextColumn<Country, string>("Country", x => x.Name, (r, v) => r.Name = v, new GridLength(6, GridUnitType.Star), new()
                    {
                        IsTextSearchEnabled = true,
                    }),
                    new TemplateColumn<Country>("Region", "RegionCell", "RegionEditCell"),
                    new TextColumn<Country, int>("Population", x => x.Population, new GridLength(3, GridUnitType.Star),
                        new TextColumnOptions<Country> { StringFormat = "{0:N}" }),
                    new TextColumn<Country, int>("Area", x => x.Area, new GridLength(3, GridUnitType.Star)),
                    new TextColumn<Country, int>("GDP", x => x.GDP, new GridLength(3, GridUnitType.Star), new()
                    {
                        TextAlignment = Avalonia.Media.TextAlignment.Right,
                        MaxWidth = new GridLength(150)
                    }),
                }
            };
            Source.RowSelection!.SingleSelect = false;
        }

        public bool CellSelection
        {
            get => _cellSelection;
            set
            {
                if (_cellSelection != value)
                {
                    _cellSelection = value;
                    if (_cellSelection)
                        Source.Selection = new TreeDataGridCellSelectionModel<Country>(Source) { SingleSelect = false };
                    else
                        Source.Selection = new TreeDataGridRowSelectionModel<Country>(Source) { SingleSelect = false };
                    this.RaisePropertyChanged();
                }
            }
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
