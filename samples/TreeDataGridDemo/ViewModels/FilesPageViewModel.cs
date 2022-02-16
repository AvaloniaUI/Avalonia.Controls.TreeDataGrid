using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using TreeDataGridDemo.Models;
using ReactiveUI;

namespace TreeDataGridDemo.ViewModels
{
    internal class FilesPageViewModel : ReactiveObject
    {
        private readonly Bitmap _folderIcon;
        private readonly Bitmap _folderOpenIcon;
        private readonly Bitmap _fileIcon;
        private FileTreeNodeModel? _root;
        private string _selectedDrive;
        private string? _selectedPath;

        public FilesPageViewModel()
        {
            var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

            using (var s = assetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/file.png")))
                _fileIcon = new Bitmap(s);
            using (var s = assetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/folder.png")))
                _folderIcon = new Bitmap(s);
            using (var s = assetLoader.Open(new Uri("avares://TreeDataGridDemo/Assets/folder-open.png")))
                _folderOpenIcon = new Bitmap(s);

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
                            new FuncDataTemplate<FileTreeNodeModel>(FileNameTemplate),
                            new GridLength(1, GridUnitType.Star),
                            new ColumnOptions<FileTreeNodeModel>
                            {
                                CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                                CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                            }),
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
            var icon = node.IsDirectory ?
                node.WhenAnyValue(x => x.IsExpanded).Select(x => x ? _folderOpenIcon : _folderIcon) :
                Avalonia.Reactive.ObservableEx.SingleValue(_fileIcon);

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new Image
                    {
                        [!Image.SourceProperty] = icon.ToBinding(),
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
    }
}
