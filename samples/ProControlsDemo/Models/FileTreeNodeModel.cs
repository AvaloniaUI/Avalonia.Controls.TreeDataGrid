using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Avalonia.Threading;
using ReactiveUI;

namespace ProControlsDemo.Models
{
    internal class FileTreeNodeModel : ReactiveObject
    {
        private string _path;
        private string _name;
        private FileSystemWatcher? _watcher;
        private ObservableCollection<FileTreeNodeModel>? _children;

        public FileTreeNodeModel(
            string path,
            bool isDirectory,
            bool isRoot = false)
        {
            _path = path;
            _name = isRoot ? path : System.IO.Path.GetFileName(Path);
            IsDirectory = isDirectory;

            if (!isDirectory)
            {
                var info = new FileInfo(path);
                Size = info.Length;
                Modified = info.LastWriteTimeUtc;
            }
        }

        public string Path 
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public string Name 
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public bool IsDirectory { get; }
        public long? Size { get; }
        public DateTimeOffset? Modified { get; }
        public IReadOnlyList<FileTreeNodeModel> Children => _children ??= LoadChildren();

        private ObservableCollection<FileTreeNodeModel> LoadChildren()
        {
            if (!IsDirectory)
            {
                throw new NotSupportedException();
            }

            var options = new EnumerationOptions { IgnoreInaccessible = true };
            var result = new ObservableCollection<FileTreeNodeModel>();

            foreach (var d in Directory.EnumerateDirectories(Path, "*", options))
            {
                result.Add(new FileTreeNodeModel(d, true));
            }

            foreach (var f in Directory.EnumerateFiles(Path, "*", options))
            {
                result.Add(new FileTreeNodeModel(f, false));
            }

            _watcher = new FileSystemWatcher
            {
                Path = Path,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
            };

            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.EnableRaisingEvents = true;

            return result;
        }

        public static Comparison<FileTreeNodeModel> SortAscending<T>(Func<FileTreeNodeModel, T> selector)
        {
            return (x, y) =>
            {
                if (x.IsDirectory == y.IsDirectory)
                    return Comparer<T>.Default.Compare(selector(x), selector(y));
                else if (x.IsDirectory)
                    return -1;
                else
                    return 1;
            };
        }

        public static Comparison<FileTreeNodeModel> SortDescending<T>(Func<FileTreeNodeModel, T> selector)
        {
            return (x, y) =>
            {
                if (x.IsDirectory == y.IsDirectory)
                    return Comparer<T>.Default.Compare(selector(y), selector(x));
                else if (x.IsDirectory)
                    return -1;
                else
                    return 1;
            };
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var node = new FileTreeNodeModel(
                    e.FullPath,
                    File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory));
                _children!.Add(node);
            });
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                for (var i = 0; i < _children!.Count; ++i)
                {
                    if (_children[i].Path == e.FullPath)
                    {
                        _children.RemoveAt(i);
                        break;
                    }
                }
            });
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var child in _children)
                {
                    if (child.Path == e.FullPath)
                    {
                        child.Path = e.FullPath;
                        child.Name = e.Name;
                        break;
                    }
                }
            });
        }
    }
}
