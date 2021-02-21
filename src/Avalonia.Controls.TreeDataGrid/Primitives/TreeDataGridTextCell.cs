using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridTextCell : TreeDataGridCell
    {
        private TextBox? _edit;

        protected override bool CanEdit => (DataContext as ICell)?.CanEdit ?? false;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_edit is object)
            {
                _edit.KeyDown -= EditKeyDown;
                _edit.LostFocus -= EditLostFocus;
            }

            _edit = e.NameScope.Find<TextBox>("PART_Edit");

            if (_edit is object)
            {
                _edit.SelectAll();
                _edit.Focus();
                _edit.KeyDown += EditKeyDown;
                _edit.LostFocus += EditLostFocus;
            }
        }

        private void EditKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EndEdit();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CancelEdit();
                e.Handled = true;
            }
        }

        private void EditLostFocus(object sender, RoutedEventArgs e) => EndEdit();
    }
}
