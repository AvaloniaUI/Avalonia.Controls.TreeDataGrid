using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Avalonia.Controls.Primitives
{
    public class TreeDataGridTextCell : TreeDataGridCell
    {
        public static readonly DirectProperty<TreeDataGridTextCell, TextTrimming> TextTrimmingProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, TextTrimming>(
                nameof(TextTrimming),
                o => o.TextTrimming);

        public static readonly DirectProperty<TreeDataGridTextCell, string?> ValueProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, string?>(
                nameof(Value),
                o => o.Value,
                (o, v) => o.Value = v);

        public static readonly DirectProperty<TreeDataGridTextCell,TextAlignment> TextAlignmentProperty =
            AvaloniaProperty.RegisterDirect < TreeDataGridTextCell, TextAlignment>(
                nameof(TextAlignment),
                o => o.TextAlignment,
                (o,v)=> o.TextAlignment = v);

        private bool _canEdit;
        private string? _value;
        private TextBox? _edit;
        private TextTrimming _textTrimming = TextTrimming.CharacterEllipsis;
        private TextAlignment _textAlignment = TextAlignment.Left;

        public TextTrimming TextTrimming
        {
            get => _textTrimming;
            set => SetAndRaise(TextTrimmingProperty, ref _textTrimming, value);
        }

        public string? Value
        {
            get => _value;
            set
            {
                if (SetAndRaise(ValueProperty, ref _value, value) && Model is ITextCell cell)
                    cell.Text = _value;
            }
        }

        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set => SetAndRaise(TextAlignmentProperty, ref _textAlignment, value);
        }

        protected override bool CanEdit => _canEdit;

        public override void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            _canEdit = model.CanEdit;
            Value = model.Value?.ToString();
            TextTrimming = (model as ITextCell)?.TextTrimming ?? TextTrimming.CharacterEllipsis;
            TextAlignment = (model as ITextCell)?.TextAlignment ?? TextAlignment.Left;
            base.Realize(factory, selection, model, columnIndex, rowIndex);
            SubscribeToModelChanges();
        }

        public override void Unrealize()
        {
            UnsubscribeFromModelChanges();
            base.Unrealize();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_edit is not null)
            {
                _edit.KeyDown -= EditKeyDown;
                _edit.LostFocus -= EditLostFocus;
            }

            _edit = e.NameScope.Find<TextBox>("PART_Edit");

            if (_edit is not null)
            {
                _edit.SelectAll();
                _edit.Focus();
                _edit.KeyDown += EditKeyDown;
                _edit.LostFocus += EditLostFocus;
            }
        }

        protected override void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);

            if (e.PropertyName == nameof(ITextCell.Value))
                Value = Model?.Value?.ToString();
        }

        private void EditKeyDown(object? sender, KeyEventArgs e)
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

        private void EditLostFocus(object? sender, RoutedEventArgs e) => EndEdit();
    }
}
