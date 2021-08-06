using System;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace UiAvalonia.Table.CellPanels
{
    internal class CheckBoxTextCellPanel : DockPanel
    {
        internal CheckBoxTextCellPanel(
            CheckBox checkBox,
            TextBlock textBlock)
        {
            mCheckBox = checkBox;
            mTextBlock = textBlock;
        }

        internal void SetData(
            bool? isChecked,
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

            mTextBlock.Text = text;
            mTextBlock.Foreground = brush;
            mTextBlock.FontWeight = fontWeight;
        }

        EventHandler<RoutedEventArgs> mEventHandler;

        readonly CheckBox mCheckBox;
        readonly TextBlock mTextBlock;
    }
}
