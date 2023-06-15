﻿using System.ComponentModel;
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

        public static readonly DirectProperty<TreeDataGridTextCell, TextWrapping> TextWrappingProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, TextWrapping>(
                nameof(TextWrapping),
                o => o.TextWrapping);

        public static readonly DirectProperty<TreeDataGridTextCell, string?> ValueProperty =
            AvaloniaProperty.RegisterDirect<TreeDataGridTextCell, string?>(
                nameof(Value),
                o => o.Value,
                (o, v) => o.Value = v);

        private string? _value;
        private TextBox? _edit;
        private TextTrimming _textTrimming = TextTrimming.CharacterEllipsis;
        private TextWrapping _textWrapping = TextWrapping.NoWrap;

        public TextTrimming TextTrimming
        {
            get => _textTrimming;
            set => SetAndRaise(TextTrimmingProperty, ref _textTrimming, value);
        }

        public TextWrapping TextWrapping
        {
            get => _textWrapping;
            set => SetAndRaise(TextWrappingProperty, ref _textWrapping, value);
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

        public override void Realize(
            TreeDataGridElementFactory factory,
            ITreeDataGridSelectionInteraction? selection,
            ICell model,
            int columnIndex,
            int rowIndex)
        {
            Value = model.Value?.ToString();
            TextTrimming = (model as ITextCell)?.TextTrimming ?? TextTrimming.CharacterEllipsis;
            TextWrapping = (model as ITextCell)?.TextWrapping ?? TextWrapping.NoWrap;
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

            _edit = e.NameScope.Find<TextBox>("PART_Edit");

            if (_edit is not null)
            {
                _edit.SelectAll();
                _edit.Focus();
            }
        }

        protected override void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);

            if (e.PropertyName == nameof(ITextCell.Value))
                Value = Model?.Value?.ToString();
        }
    }
}
