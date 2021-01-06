using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface IExpanderRowController<TModel>
    {
        void OnChildCollectionChanged(ExpanderRowBase<TModel> row, NotifyCollectionChangedEventArgs e);
    }
}
