using System;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridElementFactory : IElementFactory
    {
        private readonly RecyclePool _recyclePool = new();

        public IControl Build(object? data)
        {
            var result = GetElement(data, null);
            result.DataContext = data;
            return result;
        }

        public IControl GetElement(ElementFactoryGetArgs args) => GetElement(args.Data, args.Parent);

        public bool Match(object? data) => data is ICell;

        public void RecycleElement(ElementFactoryRecycleArgs args)
        {
            if (args.Element is not null)
            {
                _recyclePool.PutElement(args.Element, GetElementRecycleKey(args.Element), args.Parent);
            }
        }

        protected virtual IControl CreateElement(object? data)
        {
            return data switch
            {
                TemplateCell _ => new TreeDataGridTemplateCell(),
                IExpanderCell _ => new TreeDataGridExpanderCell(),
                ICell _ => new TreeDataGridTextCell(),
                IColumn _ => new TreeDataGridColumnHeader(),
                IRow _ => new TreeDataGridRow(),
                _ => throw new NotSupportedException(),
            };
        }

        protected virtual string GetDataRecycleKey(object? data)
        {
            return data switch
            {
                TemplateCell _ => typeof(TreeDataGridTemplateCell).FullName!,
                IExpanderCell _ => typeof(TreeDataGridExpanderCell).FullName!,
                ICell _ => typeof(TreeDataGridTextCell).FullName!,
                IColumn _ => typeof(TreeDataGridColumnHeader).FullName!,
                IRow _ => typeof(TreeDataGridRow).FullName!,
                _ => throw new NotSupportedException(),
            };
        }

        protected virtual string GetElementRecycleKey(IControl element)
        {
            return element.GetType().FullName!;
        }

        private IControl GetElement(object? data, IControl? parent)
        {
            var recycleKey = GetDataRecycleKey(data);

            if (_recyclePool.TryGetElement(recycleKey, parent) is IControl element)
            {
                return element;
            }

            return CreateElement(data);
        }
    }
}
