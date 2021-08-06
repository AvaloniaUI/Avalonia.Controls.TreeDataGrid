using Avalonia.Media;

namespace UiAvalonia.Table
{
    internal class CheckeablePlasticTableCell<T> :
        PlasticTableCell where T : class
    {
        internal delegate void OnCheckBoxClicked(T node, bool isChecked);

        internal readonly bool? IsChecked;
        internal readonly OnCheckBoxClicked OnCheckBoxClickedDelegate;

        internal CheckeablePlasticTableCell(
            bool? isChecked,
            string text,
            IImage image,
            StyleType style,
            ColorType color,
            OnCheckBoxClicked onCheckBoxClickedDelegate)
            : base(text, image, style, color)
        {
            IsChecked = isChecked;
            OnCheckBoxClickedDelegate = onCheckBoxClickedDelegate;
        }
    }

    internal class PlasticTableCell
    {
        internal enum StyleType { Normal, Lighter, Bold }
        internal enum ColorType { Regular, Green, Red }

        internal readonly string Text;
        internal readonly IImage Image;
        internal readonly StyleType Style;
        internal readonly ColorType Color;

        internal PlasticTableCell(string text)
            : this(text, StyleType.Normal)
        {
        }

        internal PlasticTableCell(string text, StyleType style)
            : this(text, null, style, ColorType.Regular)
        {
        }

        internal PlasticTableCell(string text, IImage image)
            : this(text, image, StyleType.Normal, ColorType.Regular)
        {
        }

        internal PlasticTableCell(string text, IImage image, StyleType style)
            : this(text, image, style, ColorType.Regular)
        {
        }

        internal PlasticTableCell(
            string text,
            IImage image,
            StyleType style,
            ColorType color)
        {
            Text = text;
            Image = image;
            Style = style;
            Color = color;
        }
    }
}