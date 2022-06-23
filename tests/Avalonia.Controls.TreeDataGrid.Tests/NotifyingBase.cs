using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Avalonia.Controls.TreeDataGridTests
{
    internal class NotifyingBase : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler? _propertyChanged;
        private int _propertyChangedCount;

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                _propertyChanged += value;
                ++_propertyChangedCount;
            }
            remove
            {
                _propertyChanged -= value;
                --_propertyChangedCount;
            }
        }

        protected bool RaiseAndSetIfChanged<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        public int PropertyChangedSubscriberCount() => _propertyChangedCount;

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            _propertyChanged?.Invoke(this, e);
        }
    }
}
