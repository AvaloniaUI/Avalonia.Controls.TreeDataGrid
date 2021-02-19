using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TextCell<T> : NotifyingBase, ICell, IDisposable
    {
        private IDisposable? _subscription;
        [AllowNull] private T _value;

        public TextCell(T value)
        {
            _value = value;
        }

        public TextCell(IObservable<BindingValue<T>> binding)
        {
            _subscription = binding.Subscribe(x =>
            {
                if (x.HasValue)
                    Value = x.Value;
            });
        }

        public T Value 
        {
            get => _value;
            private set => RaiseAndSetIfChanged(ref _value, value);
        }

        object? ICell.Value => Value;

        public void Dispose() => _subscription?.Dispose();
    }
}
