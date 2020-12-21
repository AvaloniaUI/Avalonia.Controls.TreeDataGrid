using System;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridElementFactory : IElementFactory
    {
        private readonly RecyclePool _recyclePool = new RecyclePool();

        public IControl Build(object param)
        {
            return GetElement(new ElementFactoryGetArgs { Data = param });
        }

        public IControl GetElement(ElementFactoryGetArgs args)
        {
            var recycleKey = args.Data switch
            {
                IExpanderCell _ => typeof(TreeDataGridExpanderCell).FullName,
                ICell _ => typeof(TreeDataGridTextCell).FullName,
                _ => throw new NotSupportedException(),
            };

            if (_recyclePool.TryGetElement(recycleKey, args.Parent) is IControl element)
            {
                return element;
            }

            return args.Data switch
            {
                IExpanderCell _ => new TreeDataGridExpanderCell(),
                ICell _ => new TreeDataGridTextCell(),
                _ => throw new NotSupportedException(),
            };
        }

        public bool Match(object data) => true;

        public void RecycleElement(ElementFactoryRecycleArgs args)
        {
            _recyclePool.PutElement(args.Element, args.Element.GetType().FullName, args.Parent);
        }

        public static void SetCellIndex(TreeDataGridCell cell, int columnIndex, int rowIndex)
        {
            cell.ColumnIndex = columnIndex;
            cell.RowIndex = rowIndex;
        }
    }
}
