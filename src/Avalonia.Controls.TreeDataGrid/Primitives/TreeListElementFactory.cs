using System;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Primitives
{
    public class TreeListElementFactory : IElementFactory
    {
        private readonly RecyclePool _recyclePool = new RecyclePool();

        public IControl Build(object data)
        {
            var result = GetElement(data, null);
            result.DataContext = data;
            return result;
        }

        public IControl GetElement(ElementFactoryGetArgs args) => GetElement(args.Data, args.Parent);

        public bool Match(object data) => data is ICell;

        public void RecycleElement(ElementFactoryRecycleArgs args)
        {
            _recyclePool.PutElement(args.Element, args.Element.GetType().FullName, args.Parent);
        }

        private IControl GetElement(object? data, IControl? parent)
        {
            var recycleKey = data switch
            {
                TemplateCell _ => typeof(TreeDataGridTemplateCell).FullName,
                IExpanderCell _ => typeof(TreeDataGridExpanderCell).FullName,
                ICell _ => typeof(TreeDataGridTextCell).FullName,
                _ => throw new NotSupportedException(),
            };

            if (_recyclePool.TryGetElement(recycleKey, parent) is IControl element)
            {
                return element;
            }

            return data switch
            {
                TemplateCell _ => new TreeDataGridTemplateCell(),
                IExpanderCell _ => new TreeDataGridExpanderCell(),
                ICell _ => new TreeDataGridTextCell(),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
