using System;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace UiAvalonia.Table.CellPanels
{
    internal class CheckBoxImageTextCellPanel : DockPanel
    {
        internal CheckBoxImageTextCellPanel(
            CheckBox checkBox,
            Image image,
            TextBlock textBlock)
        {
            mCheckBox = checkBox;
            mImage = image;
            mTextBlock = textBlock;
        }

        internal void SetData(
            bool? isChecked,
            IImage source,
            string text,
            IBrush brush,
            FontWeight fontWeight,
            EventHandler<RoutedEventArgs> eventHandler)
        {
            mCheckBox.IsChecked = isChecked;

            if (mEventHandler != null)
            {

                mCheckBox.Click -= mEventHandler;
            }

            mEventHandler = eventHandler;

            
                mCheckBox.Click += mEventHandler;

            mImage.Source = source;

            mTextBlock.Text = text;
            mTextBlock.Foreground = brush;
            mTextBlock.FontWeight = fontWeight;
        }

        EventHandler<RoutedEventArgs> mEventHandler;

        readonly CheckBox mCheckBox;
        readonly Image mImage;
        readonly TextBlock mTextBlock;
    }
}
