using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TreeDataGridDemo.Models
{
    public class DragDropItem : ObservableObject
    {
        private static Random _random = new Random(0);
        private ObservableCollection<DragDropItem>? _children;
        private bool _allowDrag = true;
        private bool _allowDrop = true;

        public DragDropItem(string name) => Name = name;
        public string Name { get; }

        public bool AllowDrag
        {
            get => _allowDrag;
            set => SetProperty(ref _allowDrag, value);
        }

        public bool AllowDrop
        {
            get => _allowDrop;
            set => SetProperty(ref _allowDrop, value);
        }

        public ObservableCollection<DragDropItem> Children => _children ??= CreateRandomItems();

        public static ObservableCollection<DragDropItem> CreateRandomItems()
        {
            var names = new Bogus.DataSets.Name();
            var count = _random.Next(10);
            return new ObservableCollection<DragDropItem>(Enumerable.Range(0, count)
                .Select(x => new DragDropItem(names.FullName())));
        }
    }
}
