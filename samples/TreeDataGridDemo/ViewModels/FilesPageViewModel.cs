using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using TreeDataGridDemo.Models;

namespace TreeDataGridDemo.ViewModels
{
    internal class FilesPageViewModel : ReactiveObject
    {
        private FileTreeNodeModel? _root;
        private string _selectedDrive;
        private string? _selectedPath;
        private FolderIconConverter _folderIconConverter;

        public FilesPageViewModel()
        {
            var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

            using (var fileStream = assetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/file.png")))
            using (var folderStream = assetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/folder.png")))
            using (var folderOpenStream = assetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/folder-open.png")))
            {
                var fileIcon = new Bitmap(fileStream);
                var folderIcon = new Bitmap(folderStream);
                var folderOpenIcon = new Bitmap(folderOpenStream);

                _folderIconConverter = new FolderIconConverter(fileIcon, folderOpenIcon, folderIcon);
            }

            Drives = DriveInfo.GetDrives().Select(x => x.Name).ToList();
            _selectedDrive = "C:\\";

            Source = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(Array.Empty<FileTreeNodeModel>())
            {
                Columns =
                {
                    new TemplateColumn<FileTreeNodeModel>(
                        null,
                        new FuncDataTemplate<FileTreeNodeModel>(FileCheckTemplate, true),
                        options: new ColumnOptions<FileTreeNodeModel>
                        {
                            CanUserResizeColumn = false,
                        }),
                    new HierarchicalExpanderColumn<FileTreeNodeModel>(
                        new TemplateColumn<FileTreeNodeModel>(
                            "Name",
                            new FuncDataTemplate<FileTreeNodeModel>(FileNameTemplate, true),
                            new GridLength(1, GridUnitType.Star),
                            new ColumnOptions<FileTreeNodeModel>
                            {
                                CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                                CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                            })
                        {
                            IsTextSearchEnabled = true,
                            TextSearchValueSelector = x => x.Name
                        },
                        x => x.Children,
                        x => x.IsDirectory,
                        x => x.IsExpanded),
                    new TextColumn<FileTreeNodeModel, long?>(
                        "Size",
                        x => x.Size,
                        options: new TextColumnOptions<FileTreeNodeModel>
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                        }),
                    new TextColumn<FileTreeNodeModel, DateTimeOffset?>(
                        "Modified",
                        x => x.Modified,
                        options: new TextColumnOptions<FileTreeNodeModel>
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Modified),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Modified),
                        }),
                }
            };

            Source.RowSelection!.SingleSelect = false;
            Source.RowSelection.SelectionChanged += SelectionChanged;

            this.WhenAnyValue(x => x.SelectedDrive)
                .Subscribe(x =>
                {
                    _root = new FileTreeNodeModel(_selectedDrive, isDirectory: true, isRoot: true);
                    Source.Items = new[] { _root };
                });
        }

        public IList<string> Drives { get; }

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

        public HierarchicalTreeDataGridSource<FileTreeNodeModel> Source { get; }

        private IControl FileCheckTemplate(FileTreeNodeModel node, INameScope ns)
        {
            return new CheckBox
            {
                MinWidth = 0,
                [!CheckBox.IsCheckedProperty] = new Binding(nameof(FileTreeNodeModel.IsChecked)),
            };
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
                        [!Image.SourceProperty] = new MultiBinding
                        {
                            Bindings =
                            {
                                new Binding(nameof(node.IsDirectory)),
                                new Binding(nameof(node.IsExpanded)),
                            },
                            Converter = _folderIconConverter,
                        },
                        Margin = new Thickness(0, 0, 4, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    new TextBlock
                    {
                        [!TextBlock.TextProperty] = new Binding(nameof(FileTreeNodeModel.Name)),
                        VerticalAlignment = VerticalAlignment.Center,
                    }
                }
            };
        }

        private void SetSelectedPath(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Source.RowSelection!.Clear();
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

            Source.RowSelection!.SelectedIndex = index;
        }

        private void SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<FileTreeNodeModel> e)
        {
            var selectedPath = Source.RowSelection?.SelectedItem?.Path;
            this.RaiseAndSetIfChanged(ref _selectedPath, selectedPath, nameof(SelectedPath));

            foreach (var i in e.DeselectedItems)
                System.Diagnostics.Trace.WriteLine($"Deselected '{i?.Path}'");
            foreach (var i in e.SelectedItems)
                System.Diagnostics.Trace.WriteLine($"Selected '{i?.Path}'");
        }

        private class FolderIconConverter : IMultiValueConverter
        {
            private readonly Bitmap _file;
            private readonly Bitmap _folderExpanded;
            private readonly Bitmap _folderCollapsed;

            public FolderIconConverter(Bitmap file, Bitmap folderExpanded, Bitmap folderCollapsed)
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
