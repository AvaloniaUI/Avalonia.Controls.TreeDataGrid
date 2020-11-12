using System;
using System.Collections.Generic;
using System.IO;

namespace ProControlsDemo.Models
{
    class TreeNodeModel
    {
        private IReadOnlyList<TreeNodeModel>? _children;

        public TreeNodeModel(
            string path,
            bool isDirectory,
            bool isRoot = false)
        {
            Path = path;
            IsDirectory = isDirectory;
            Name = isRoot ? path : System.IO.Path.GetFileName(Path);

            if (!isDirectory)
            {
                var info = new FileInfo(path);
                Size = info.Length;
                Modified = info.LastWriteTimeUtc;
            }
        }

        public string Path { get; }
        public bool IsDirectory { get; }
        public string Name { get; }
        public long? Size { get; }
        public DateTimeOffset? Modified { get; }

        public IReadOnlyList<TreeNodeModel> Children => _children ??= LoadChildren();

        private IReadOnlyList<TreeNodeModel> LoadChildren()
        {
            if (!IsDirectory)
            {
                return Array.Empty<TreeNodeModel>();
            }

            var options = new EnumerationOptions { IgnoreInaccessible = true };
            var result = new List<TreeNodeModel>();

            foreach (var d in Directory.EnumerateDirectories(Path, "*", options))
            {
                result.Add(new TreeNodeModel(d, true));
            }

            foreach (var f in Directory.EnumerateFiles(Path, "*", options))
            {
                result.Add(new TreeNodeModel(f, false));
            }

            return result;
        }
    }
}
