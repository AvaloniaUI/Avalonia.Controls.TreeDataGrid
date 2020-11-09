
namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TextCell<T> : ICell
    {
        public TextCell(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
