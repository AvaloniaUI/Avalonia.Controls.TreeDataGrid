using Avalonia.Collections;
using Avalonia.Diagnostics;

namespace Avalonia.Controls.TreeDataGridTests
{
    internal static class CollectionExtensions
    {
        public static int CollectionChangedSubscriberCount<T>(this AvaloniaListDebug<T> list)
        {
            return list.GetCollectionChangedSubscribers()?.Length ?? 0;
        }
    }
}
