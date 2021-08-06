using Avalonia.Controls;
using Avalonia.Media;

namespace UiAvalonia.Table.CellPanels
{
    internal class TextCellPanel : DockPanel
    {
        internal TextCellPanel(TextBlock textBlock)
        {
            mTextBlock = textBlock;
        }

        internal void SetData(
            string text,
            IBrush brush,
            FontWeight fontWeight)
        {
            mTextBlock.Text = text;
            mTextBlock.Foreground = brush;
            mTextBlock.FontWeight = fontWeight;
        }

        readonly TextBlock mTextBlock;
    }
}
