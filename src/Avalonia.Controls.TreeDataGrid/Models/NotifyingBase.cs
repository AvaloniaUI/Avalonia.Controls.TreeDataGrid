using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Avalonia.Controls.Models
{
    public class NotifyingBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
