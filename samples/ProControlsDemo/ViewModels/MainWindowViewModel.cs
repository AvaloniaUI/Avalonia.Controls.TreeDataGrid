using Avalonia.Controls;
using ProControlsDemo.Models;

namespace ProControlsDemo.ViewModels
{
    internal class MainWindowViewModel
    {
        private HierarchicalTreeDataGridSource<TreeNodeModel>? _files;

        public HierarchicalTreeDataGridSource<TreeNodeModel> Files
        {
            get
            {
                if (_files is null)
                {
                    var model = new TreeNodeModel(@"c:\", isDirectory: true, isRoot: true);
                    var source = new HierarchicalTreeDataGridSource<TreeNodeModel>(model, x => x.Children, x => x.IsDirectory);
                    source.AddColumn("Name", x => x.Name);
                    source.AddColumn("Size", x => x.Size, GridLength.Auto);
                    source.AddColumn("Modified", x => x.Modified, GridLength.Auto);
                    _files = source;
                }

                return _files;
            }
        }
    }
}
