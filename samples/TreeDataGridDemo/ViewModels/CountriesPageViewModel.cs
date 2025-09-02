using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Media;
using ReactiveUI;
using TreeDataGridDemo.Models;
using TreeDataGridDemo.ViewModels;

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
                    // Text column with advanced filter
                    new TextColumn<Country, string?>(
                        "Country",
                        x => x.Name,
                        (r, v) => r.Name = v,
                        new GridLength(6, GridUnitType.Star),
                        new TextColumnOptions<Country>
                        {
                            IsTextSearchEnabled = true,
                            IsFilterEnabled = true // This will create a TextFilter automatically
                        }),

                    // Use a template column with filtering for region
                    new TemplateColumn<Country>(
                        "Region",
                        "RegionCell",
                        "RegionEditCell",
                        new GridLength(3, GridUnitType.Star),
                        new TemplateColumnOptions<Country>
                        {
                            // Value selector for filtering
                            FilterValueSelector = x => x.Region,
                            Filter = new TextValueFilter(),
                            FilterControlFactory = (col) => new TextFilterControl(col, "custom filter..."),
                        }),

                    // Population column with numeric filtering
                    new TextColumn<Country, int>(
                        "Population",
                        x => x.Population,
                        new GridLength(3, GridUnitType.Star),
                        new TextColumnOptions<Country> { IsFilterEnabled = true }),

                    // Area column with numeric filtering
                    new TextColumn<Country, int>(
                        "Area",
                        x => x.Area,
                        new GridLength(3, GridUnitType.Star),
                        new TextColumnOptions<Country> { IsFilterEnabled = true, StringFormat = "{0:N0}" }),

                    // GDP column
                    new TextColumn<Country, int>(
                        "GDP",
                        x => x.GDP,
                        new GridLength(3, GridUnitType.Star),
                        new TextColumnOptions<Country>
                        {
                            TextAlignment = TextAlignment.Right,
                            MaxWidth = new GridLength(150),
                            IsFilterEnabled = true,
                            StringFormat = "${0:N0}"
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
