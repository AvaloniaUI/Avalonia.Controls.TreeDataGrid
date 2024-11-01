using System;
using System.ComponentModel;
using System.Globalization;
using System.Reactive.Subjects;
using Avalonia.Data;
using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TextCell<T> : NotifyingBase, ITextCell, IDisposable, IEditableObject
    {
        private readonly ISubject<BindingValue<T>>? _binding;
        private readonly ITextCellOptions? _options;
        private readonly IDisposable? _subscription;
        private string? _editText;
        private T? _value;
        private bool _isEditing;

        public TextCell(T? value)
        {
            _value = value;
            IsReadOnly = true;
        }

        public TextCell(
            ISubject<BindingValue<T>> binding,
            bool isReadOnly,
            ITextCellOptions? options = null)
        {
            _binding = binding;
            IsReadOnly = isReadOnly;
            _options = options;

            _subscription = binding.Subscribe(x =>
            {
                if (x.HasValue)
                    Value = x.Value;
            });
        }

        public bool CanEdit => !IsReadOnly;
        public BeginEditGestures EditGestures => _options?.BeginEditGestures ?? BeginEditGestures.Default;
        public bool IsReadOnly { get; }
        public TextTrimming TextTrimming => _options?.TextTrimming ?? TextTrimming.None;
        public TextWrapping TextWrapping => _options?.TextWrapping ?? TextWrapping.NoWrap;
        public TextAlignment TextAlignment => _options?.TextAlignment ?? TextAlignment.Left;

        public string? Text
        {
            get
            {
                if (_isEditing)
                    return _editText;
                else if (_options?.StringFormat is { } format)
                    return string.Format(_options.Culture ?? CultureInfo.CurrentCulture, format, _value);
                else
                    return _value?.ToString();
            }
            set
            {
                if (_isEditing)
                {
                    _editText = value;
                }
                else
                {
                    try
                    {
                        Value = (T?)Convert.ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        // TODO: Data validation errors.
                    }
                }
            }
        }

        public T? Value
        {
            get => _value;
            set
            {
                if (RaiseAndSetIfChanged(ref _value, value) && !IsReadOnly && !_isEditing)
                    _binding!.OnNext(value!);
            }
        }

        object? ICell.Value => Value;

        public void BeginEdit()
        {
            if (!_isEditing && !IsReadOnly)
            {
                _isEditing = true;
                _editText = Text;
            }
        }

        public void CancelEdit()
        {
            if (_isEditing)
            {
                _isEditing = false;
                _editText = null;
            }
        }

        public void EndEdit()
        {
            if (_isEditing)
            {
                var text = _editText;
                _isEditing = false;
                _editText = null;
                Text = text;
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
