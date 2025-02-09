using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ReactiveUI;
using TreeDataGridDemo.Models;

namespace TreeDataGridDemo.ViewModels
{
    internal class DragDropPageViewModel : ReactiveObject
    {
        private ObservableCollection<DragDropItem> _data;

        public DragDropPageViewModel()
        {
            _data = DragDropItem.CreateRandomItems();
            var source = new HierarchicalTreeDataGridSource<DragDropItem>(_data)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<DragDropItem>(
                        new TextColumn<DragDropItem, string>(
                            "Name",
                            x => x.Name,
                            GridLength.Star),
                        x => x.Children,
                        x => x.Children.Count > 0,
                        x => x.IsExpanded),
                    new CheckBoxColumn<DragDropItem>(
                        "Allow Drag",
                        x => x.AllowDrag,
                        (o, x) => o.AllowDrag = x),
                    new CheckBoxColumn<DragDropItem>(
                        "Allow Drop",
                        x => x.AllowDrop,
                        (o, x) => o.AllowDrop = x),
                }
            };

            source.RowSelection!.SingleSelect = false;
            Source = source;
        }

        public ITreeDataGridSource<DragDropItem> Source { get; }
    }
}
