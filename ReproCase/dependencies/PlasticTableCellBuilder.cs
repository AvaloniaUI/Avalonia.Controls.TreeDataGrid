using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

using PlasticGui;

using UiAvalonia.Table.CellPanels;

namespace UiAvalonia.Table
{
    internal static class PlasticTableCellBuilder
    {
        internal static IControl CreateCellControl<T>(
            PlasticTableColumn.Render renderType,
            PlasticTableCell cell,
            T node) where T : class
        {
            if (renderType == PlasticTableColumn.Render.CheckBoxIconAndText)
            {
                CheckeablePlasticTableCell<T> checkableCell =
                    (CheckeablePlasticTableCell<T>)cell;

                CheckBoxImageTextCellPanel checkBoxImageTextPanel =
                    CreateCheckboxImageTextPanel(
                        checkableCell.Style,
                        checkableCell.Color);

                UpdateCheckBoxImageTextCellControl(
                    checkBoxImageTextPanel, checkableCell, node);

                return checkBoxImageTextPanel;
            }

            if (renderType == PlasticTableColumn.Render.IconAndText)
            {
                ImageTextCellPanel imageTextPanel =
                    CreateImageTextPanel(cell.Style, cell.Color);

                UpdateImageTextCellControl(imageTextPanel, cell);

                return imageTextPanel;
            }

            if (renderType == PlasticTableColumn.Render.CheckBoxAndText)
            {
                CheckeablePlasticTableCell<T> checkableCell =
                    (CheckeablePlasticTableCell<T>)cell;

                CheckBoxTextCellPanel checkBoxTextCellPanel =
                    CreateCheckBoxTextPanel(
                        checkableCell.Style,
                        checkableCell.Color);

                UpdateCheckBoxTextCellControl(
                    checkBoxTextCellPanel, checkableCell, node);

                return checkBoxTextCellPanel;
            }

            TextCellPanel textPanel =
                CreateTextPanel(cell.Style, cell.Color);

            UpdateCellControl(textPanel, cell);

            return textPanel;
        }

        internal static void UpdateCellControl<T>(
            PlasticTableColumn.Render renderType,
            PlasticTableCell cell,
            IControl cellControl,
            T node) where T : class
        {
            if (renderType == PlasticTableColumn.Render.CheckBoxIconAndText)
            {
                CheckBoxImageTextCellPanel checkBoxImageTextCellPanel =
                    cellControl.FindDescendantOfType<CheckBoxImageTextCellPanel>();

                CheckeablePlasticTableCell<T> checkableCell =
                    (CheckeablePlasticTableCell<T>)cell;

                UpdateCheckBoxImageTextCellControl(
                    checkBoxImageTextCellPanel, checkableCell, node);

                return;
            }

            if (renderType == PlasticTableColumn.Render.IconAndText)
            {
                ImageTextCellPanel imageTextCellPanel =
                    cellControl.FindDescendantOfType<ImageTextCellPanel>();

                UpdateImageTextCellControl(imageTextCellPanel, cell);

                return;
            }

            if (renderType == PlasticTableColumn.Render.CheckBoxAndText)
            {
                CheckBoxTextCellPanel checkBoxTextCellPanel =
                    cellControl.FindDescendantOfType<CheckBoxTextCellPanel>();

                CheckeablePlasticTableCell<T> checkableCell =
                    (CheckeablePlasticTableCell<T>)cell;

                UpdateCheckBoxTextCellControl(
                    checkBoxTextCellPanel, checkableCell, node);
                return;
            }

            TextCellPanel textCellPanel =
                cellControl.FindDescendantOfType<TextCellPanel>();

            UpdateCellControl(textCellPanel, cell);
        }

        internal static IBrush GetTextBlockForeground(
             PlasticTableCell.StyleType style,
             PlasticTableCell.ColorType color)
        {
            if (color == PlasticTableCell.ColorType.Red &&
                style == PlasticTableCell.StyleType.Lighter)
                return Brushes.IndianRed;

            return Brushes.Black;
        }

        internal static FontWeight GetTextBlockFontWeight(
            PlasticTableCell.StyleType style)
        {
            if (style == PlasticTableCell.StyleType.Bold)
                return FontWeight.DemiBold;

            return FontWeight.Normal;
        }

        static void UpdateCheckBoxImageTextCellControl<T>(
            CheckBoxImageTextCellPanel panel,
            CheckeablePlasticTableCell<T> cell,
            T node) where T : class
        {
            if (panel == null)
                return;

            panel.SetData(
                cell.IsChecked,
                cell.Image,
                cell.Text,
                GetTextBlockForeground(cell.Style, cell.Color),
                GetTextBlockFontWeight(cell.Style),
                (s, e) => cell.OnCheckBoxClickedDelegate(
                    node, ((CheckBox)s).IsChecked.Value));
        }

        static void UpdateImageTextCellControl(
            ImageTextCellPanel panel,
            PlasticTableCell cell)
        {
            if (panel == null)
                return;

            panel.SetData(
                cell.Image,
                cell.Text,
                GetTextBlockForeground(cell.Style, cell.Color),
                GetTextBlockFontWeight(cell.Style));
        }

        static void UpdateCheckBoxTextCellControl<T>(
            CheckBoxTextCellPanel panel,
            CheckeablePlasticTableCell<T> cell,
            T node) where T : class
        {
            if (panel == null)
                return;

            panel.SetData(
                cell.IsChecked,
                cell.Text,
                GetTextBlockForeground(cell.Style, cell.Color),
                GetTextBlockFontWeight(cell.Style),
                (s, e) => cell.OnCheckBoxClickedDelegate(
                    node, ((CheckBox)s).IsChecked.Value));
        }

        static void UpdateCellControl(
           TextCellPanel panel,
           PlasticTableCell cell)
        {
            if (panel == null)
                return;

            panel.SetData(
                cell.Text,
                GetTextBlockForeground(cell.Style, cell.Color),
                GetTextBlockFontWeight(cell.Style));
        }

        static CheckBoxImageTextCellPanel CreateCheckboxImageTextPanel(
            PlasticTableCell.StyleType style,
            PlasticTableCell.ColorType color)
        {
            CheckBox checkBox = new CheckBox();

            Image image = CreateImage(
                new Thickness(0, 0, 5, 0));

            TextBlock textBlock = CreateTextBlock(style, color);

            CheckBoxImageTextCellPanel result =
                new CheckBoxImageTextCellPanel(
                    checkBox, image, textBlock);

            SetupCellPanel(result, checkBox, image, textBlock);

            return result;
        }

        static ImageTextCellPanel CreateImageTextPanel(
            PlasticTableCell.StyleType style,
            PlasticTableCell.ColorType color)
        {
            Image image = CreateImage(new Thickness(
                10,
                0,
                5,
                0));

            TextBlock textBlock = CreateTextBlock(style, color);

            ImageTextCellPanel result = new ImageTextCellPanel(
                image, textBlock);

            SetupCellPanel(result, image, textBlock);

            return result;
        }

        static CheckBoxTextCellPanel CreateCheckBoxTextPanel(
            PlasticTableCell.StyleType style,
            PlasticTableCell.ColorType color)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Margin = new Thickness(
                10, 0,
                5, 0);

            TextBlock textBlock = CreateTextBlock(
                style, color);

            CheckBoxTextCellPanel result = new CheckBoxTextCellPanel(
                checkBox, textBlock);

            SetupCellPanel(result, checkBox, textBlock);

            return result;
        }

        static TextCellPanel CreateTextPanel(
            PlasticTableCell.StyleType style,
            PlasticTableCell.ColorType color)
        {
            TextBlock textBlock = CreateTextBlock(style, color);

            TextCellPanel result = new TextCellPanel(
                textBlock);

            SetupCellPanel(result, textBlock);

            return result;
        }

        static void SetupCellPanel(
            DockPanel parent,
            params Control[] controls)
        {
            KeyboardNavigation.SetTabNavigation(
                parent, KeyboardNavigationMode.None);

            parent.VerticalAlignment = VerticalAlignment.Center;
            parent.Height = 27;

            for (int i = 0; i < controls.Length - 1; i++)
                DockPanel.SetDock(controls[i], Dock.Left);

            parent.Children.AddRange(controls);
        }

        static Image CreateImage(Thickness margin)
        {
            Image result = new Image();
            result.VerticalAlignment = VerticalAlignment.Center;
            result.Margin = margin;
            result.Width = 24;
            result.Height = 24;
            return result;
        }

        static TextBlock CreateTextBlock(
            PlasticTableCell.StyleType style,
            PlasticTableCell.ColorType color)
        {
            TextBlock result = new TextBlock();
            result.Margin = new Thickness(0, 0, 10, 0);
            result.VerticalAlignment = VerticalAlignment.Center;
            result.Foreground = GetTextBlockForeground(style, color);
            result.FontWeight = GetTextBlockFontWeight(style);
            return result;
        }
    }
}