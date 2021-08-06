using Avalonia.Controls;
using Avalonia.Media;

namespace UiAvalonia.Table.CellPanels
{
    internal class ImageTextCellPanel : DockPanel
    {
        internal ImageTextCellPanel(
            Image image,
            TextBlock textBlock)
        {
            mImage = image;
            mTextBlock = textBlock;
        }

        internal void SetData(
            IImage source,
            string text,
            IBrush brush,
            FontWeight fontWeight)
        {
            mImage.Source = source;
            mTextBlock.Text = text;
            mTextBlock.Foreground = brush;
            mTextBlock.FontWeight = fontWeight;
        }

        readonly Image mImage;
        readonly TextBlock mTextBlock;
    }
}
