using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using TreeDataGridDemo.Models;

namespace TreeDataGridDemo.ViewModels
{
    public class FilesPageViewModel : ReactiveObject
    {
        private static IconConverter? s_iconConverter;
        private readonly HierarchicalTreeDataGridSource<FileTreeNodeModel>? _treeSource;
        private FlatTreeDataGridSource<FileTreeNodeModel>? _flatSource;
        private ITreeDataGridSource<FileTreeNodeModel> _source;
        private bool _cellSelection;
        private FileTreeNodeModel? _root;
        private string _selectedDrive;
        private string? _selectedPath;

        public FilesPageViewModel()
        {
            Drives = DriveInfo.GetDrives().Select(x => x.Name).ToList();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _selectedDrive = "C:\\";
            }
            else
            {
                _selectedDrive = Drives.FirstOrDefault() ?? "/";
            }

            _source = _treeSource = CreateTreeSource();

            this.WhenAnyValue(x => x.SelectedDrive)
                .Subscribe(x =>
                {
                    _root = new FileTreeNodeModel(_selectedDrive, isDirectory: true, isRoot: true);

                    if (_treeSource is not null)
                        _treeSource.Items = new[] { _root };
                    else if (_flatSource is not null)
                        _flatSource.Items = _root.Children;
                });
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
                        Source.Selection = new TreeDataGridCellSelectionModel<FileTreeNodeModel>(Source) { SingleSelect = false };
                    else
                        Source.Selection = new TreeDataGridRowSelectionModel<FileTreeNodeModel>(Source) { SingleSelect = false };
                    this.RaisePropertyChanged();
                }
            }
        }

        public IList<string> Drives { get; }

        public bool FlatList
        {
            get => Source != _treeSource;
            set
            {
                if (value != FlatList)
                    Source = value ? _flatSource ??= CreateFlatSource() : _treeSource!;
            }
        }

        public string SelectedDrive
        {
            get => _selectedDrive;
            set => this.RaiseAndSetIfChanged(ref _selectedDrive, value);
        }

        public string? SelectedPath
        {
            get => _selectedPath;
            set => SetSelectedPath(value);
        }

        public ITreeDataGridSource<FileTreeNodeModel> Source
        {
            get => _source;
            private set => this.RaiseAndSetIfChanged(ref _source, value);
        }

        public static IMultiValueConverter FileIconConverter
        {
            get
            {
                if (s_iconConverter is null)
                {
                    using (var fileStream = AssetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/file.png")))
                    using (var folderStream = AssetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/folder.png")))
                    using (var folderOpenStream = AssetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/folder-open.png")))
                    {
                        var fileIcon = new Bitmap(fileStream);
                        var folderIcon = new Bitmap(folderStream);
                        var folderOpenIcon = new Bitmap(folderOpenStream);

                        s_iconConverter = new IconConverter(fileIcon, folderOpenIcon, folderIcon);
                    }
                }

                return s_iconConverter;
            }
        }

        private FlatTreeDataGridSource<FileTreeNodeModel> CreateFlatSource()
        {
            var result = new FlatTreeDataGridSource<FileTreeNodeModel>(_root!.Children)
            {
                Columns =
                {
                    new CheckBoxColumn<FileTreeNodeModel>(
                        null,
                        x => x.IsChecked,
                        (o, v) => o.IsChecked = v,
                        options: new()
                        {
                            CanUserResizeColumn = false,
                        }),
                    new TemplateColumn<FileTreeNodeModel>(
                        "Name",
                        "FileNameCell",
                        "FileNameEditCell",
                        new GridLength(1, GridUnitType.Star),
                        new()
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                            IsTextSearchEnabled = true,
                            TextSearchValueSelector = x => x.Name
                        }),
                    new TextColumn<FileTreeNodeModel, long?>(
                        "Size",
                        x => x.Size,
                        options: new()
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                        }),
                    new TextColumn<FileTreeNodeModel, DateTimeOffset?>(
                        "Modified",
                        x => x.Modified,
                        options: new()
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Modified),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Modified),
                        }),
                }
            };

            result.RowSelection!.SingleSelect = false;
            result.RowSelection.SelectionChanged += SelectionChanged;
            return result;
        }

        private HierarchicalTreeDataGridSource<FileTreeNodeModel> CreateTreeSource()
        {
            var result = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(Array.Empty<FileTreeNodeModel>())
            {
                Columns =
                {
                    new CheckBoxColumn<FileTreeNodeModel>(
                        null,
                        x => x.IsChecked,
                        (o, v) => o.IsChecked = v,
                        options: new()
                        {
                            CanUserResizeColumn = false,
                        }),
                    new HierarchicalExpanderColumn<FileTreeNodeModel>(
                        new TemplateColumn<FileTreeNodeModel>(
                            "Name",
                            "FileNameCell",
                            "FileNameEditCell",
                            new GridLength(1, GridUnitType.Star),
                            new()
                            {
                                CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                                CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                                IsTextSearchEnabled = true,
                                TextSearchValueSelector = x => x.Name
                            }),
                        x => x.Children,
                        x => x.HasChildren,
                        x => x.IsExpanded),
                    new TextColumn<FileTreeNodeModel, long?>(
                        "Size",
                        x => x.Size,
                        options: new()
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                        }),
                    new TextColumn<FileTreeNodeModel, DateTimeOffset?>(
                        "Modified",
                        x => x.Modified,
                        options: new()
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Modified),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Modified),
                        }),
                }
            };

            result.RowSelection!.SingleSelect = false;
            result.RowSelection.SelectionChanged += SelectionChanged;
            return result;
        }

        private void SetSelectedPath(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                GetRowSelection(Source).Clear();
                return;
            }

            var path = value;
            var components = new Stack<string>();
            DirectoryInfo? d = null;

            if (File.Exists(path))
            {
                var f = new FileInfo(path);
                components.Push(f.Name);
                d = f.Directory;
            }
            else if (Directory.Exists(path))
            {
                d = new DirectoryInfo(path);
            }

            while (d is not null)
            {
                components.Push(d.Name);
                d = d.Parent;
            }

            var index = IndexPath.Unselected;

            if (components.Count > 0)
            {
                var drive = components.Pop();
                var driveIndex = Drives.FindIndex(x => string.Equals(x, drive, StringComparison.OrdinalIgnoreCase));

                if (driveIndex >= 0)
                    SelectedDrive = Drives[driveIndex];

                FileTreeNodeModel? node = _root;
                index = new IndexPath(0);

                while (node is not null && components.Count > 0)
                {
                    node.IsExpanded = true;

                    var component = components.Pop();
                    var i = node.Children.FindIndex(x => string.Equals(x.Name, component, StringComparison.OrdinalIgnoreCase));
                    node = i >= 0 ? node.Children[i] : null;
                    index = i >= 0 ? index.Append(i) : default;
                }
            }

            GetRowSelection(Source).SelectedIndex = index;
        }

        private ITreeDataGridRowSelectionModel<FileTreeNodeModel> GetRowSelection(ITreeDataGridSource source)
        {
            return source.Selection as ITreeDataGridRowSelectionModel<FileTreeNodeModel> ??
                throw new InvalidOperationException("Expected a row selection model.");
        }

        private void SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<FileTreeNodeModel> e)
        {
            var selectedPath = GetRowSelection(Source).SelectedItem?.Path;
            this.RaiseAndSetIfChanged(ref _selectedPath, selectedPath, nameof(SelectedPath));

            foreach (var i in e.DeselectedItems)
                System.Diagnostics.Trace.WriteLine($"Deselected '{i?.Path}'");
            foreach (var i in e.SelectedItems)
                System.Diagnostics.Trace.WriteLine($"Selected '{i?.Path}'");
        }

        private class IconConverter : IMultiValueConverter
        {
            private readonly Bitmap _file;
            private readonly Bitmap _folderExpanded;
            private readonly Bitmap _folderCollapsed;

            public IconConverter(Bitmap file, Bitmap folderExpanded, Bitmap folderCollapsed)
            {
                _file = file;
                _folderExpanded = folderExpanded;
                _folderCollapsed = folderCollapsed;
            }

            public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            {
                if (values.Count == 2 &&
                    values[0] is bool isDirectory &&
                    values[1] is bool isExpanded)
                {
                    if (!isDirectory)
                        return _file;
                    else
                        return isExpanded ? _folderExpanded : _folderCollapsed;
                }

                return null;
            }
        }
    }
}
