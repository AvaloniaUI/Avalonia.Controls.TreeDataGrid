using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridPanel : Control
    {
        public static readonly DirectProperty<TreeDataGridPanel, IControl?> HeaderProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridPanel, IControl?>(
                nameof(Header),
                o => o.Header,
                (o, v) => o.Header = v);

        public static readonly DirectProperty<TreeDataGridPanel, IControl?> ContentProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridPanel, IControl?>(
                nameof(Content),
                o => o.Content,
                (o, v) => o.Content = v);

        private IControl? _header;
        private IControl? _content;

        public IControl? Header
        { 
            get => _header;
            set => UpdateChild(HeaderProperty, ref _header, value);
        }

        [Content]
        public IControl? Content
        { 
            get => _content;
            set => UpdateChild(ContentProperty, ref _content, value);
        }

        public void InvalidateAll()
        {
            InvalidateMeasure();
            InvalidateTree(_header);
            InvalidateTree(_content);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = Size.Empty;

            if (_content is object)
            {
                _content.Measure(availableSize);
                result = _content.DesiredSize;
            }

            if (_header is object)
            {
                InvalidateTree(_header);
                _header.Measure(availableSize);
                result = new Size(
                    Math.Max(result.Width, _header.DesiredSize.Width),
                    result.Height + _header.DesiredSize.Height);

                // Arranging Grid to a size smaller than the measure constraint causes it to just use
                // the measure constraint anyway. Because of this we have to remeasure the content
                // now we know the header height.
                var maxContentHeight = availableSize.Height - _header.DesiredSize.Height;
                if (_content is object && 
                    !double.IsInfinity(availableSize.Height) &&
                    _content.DesiredSize.Height > maxContentHeight)
                {
                    _content.Measure(new Size(availableSize.Width, maxContentHeight));
                    result = result.WithHeight(_header.DesiredSize.Height + _content.DesiredSize.Height);
                }
            }

            return result;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_content is object)
            {
                var y = _header?.DesiredSize.Height ?? 0;
                _content.Arrange(new Rect(0, y, finalSize.Width, finalSize.Height - y));
            }

            if (_header is object)
            {
                _header.Arrange(new Rect(0, 0, finalSize.Width, _header.DesiredSize.Height));
            }

            return finalSize;
        }

        private void UpdateChild(AvaloniaProperty<IControl?> property, ref IControl? field, IControl? value)
        {
            var oldChild = field;

            if (SetAndRaise(property, ref field, value))
            {
                if (oldChild is object)
                {
                    LogicalChildren.Remove(oldChild);
                    VisualChildren.Remove(oldChild);
                }

                if (value is object)
                {
                    LogicalChildren.Add(value);
                    VisualChildren.Add(value);
                }

                InvalidateMeasure();
            }
        }

        private void InvalidateTree(IControl? control)
        {
            if (control is null)
                return;

            control.InvalidateMeasure();

            foreach (var child in control.VisualChildren)
            {
                if (child is IControl c)
                {
                    InvalidateTree(c);
                }
            }
        }
    }
}
