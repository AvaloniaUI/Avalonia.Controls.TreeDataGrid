using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ProControlsDemo.Models;
using ReactiveUI;

namespace ProControlsDemo.ViewModels
{
    internal class MainWindowViewModel
    {
        private static Bitmap _folderIcon;
        private static Bitmap _fileIcon;
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
                    _countries = new FlatTreeDataGridSource<Country>(_countryData)
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
                    EnsureIcons();

                    var model = new FileTreeNodeModel(@"c:\", isDirectory: true, isRoot: true);
                    var source = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(model)
                    {
                        Columns =
                        {
                            new HierarchicalExpanderColumn<FileTreeNodeModel>(
                                new TemplateColumn<FileTreeNodeModel>(
                                    "Name",
                                    new FuncDataTemplate<FileTreeNodeModel>(FileNameTemplate),
                                    new GridLength(1, GridUnitType.Star),
                                    new ColumnOptions<FileTreeNodeModel>
                                    {
                                        CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                                        CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                                    }),
                                x => x.Children,
                                x => x.IsDirectory),
                            new TextColumn<FileTreeNodeModel, long?>(
                                "Size", 
                                x => x.Size,
                                options: new ColumnOptions<FileTreeNodeModel>
                                {
                                    CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                                    CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                                }),
                            new TextColumn<FileTreeNodeModel, DateTimeOffset?>(
                                "Size",
                                x => x.Modified,
                                options: new ColumnOptions<FileTreeNodeModel>
                                {
                                    CompareAscending = FileTreeNodeModel.SortAscending(x => x.Modified),
                                    CompareDescending = FileTreeNodeModel.SortDescending(x => x.Modified),
                                }),
                        }
                    };

                    _files = source;
                }

                return _files;
            }
        }

        public void AddCountry(Country country) => _countryData?.Add(country);

        private static void EnsureIcons()
        {
            if (_fileIcon is null)
            {
                var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
                using (var s = assetLoader.Open(new Uri("avares://ProControlsDemo/Assets/file.png")))
                    _fileIcon = new Bitmap(s);
                using (var s = assetLoader.Open(new Uri("avares://ProControlsDemo/Assets/folder.png")))
                    _folderIcon = new Bitmap(s);
            }
        }

        private IControl FileNameTemplate(FileTreeNodeModel node, INameScope ns)
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new Image 
                    { 
                        Source = node.IsDirectory ? _folderIcon : _fileIcon,
                        Margin = new Thickness(0, 0, 4, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    new TextBlock 
                    { 
                        [!TextBlock.TextProperty] = new Binding("Name"),
                        VerticalAlignment = VerticalAlignment.Center,
                    }
                }
            };
        }
    }
}
