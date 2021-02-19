using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Avalonia.Controls.TreeDataGrid.Tests
{
    public class NotifyingBase : INotifyPropertyChanged
    {
        public int PropertyChangedSubscriberCount
        {
            get => PropertyChanged?.GetInvocationList().Length ?? 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
