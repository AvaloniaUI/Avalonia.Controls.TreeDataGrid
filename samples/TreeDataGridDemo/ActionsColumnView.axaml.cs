using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TreeDataGridDemo
{
    using System.Windows.Input;
    using ReactiveUI;

    public class ActionsColumnViewModel : ReactiveObject
    {
        private bool _isButtonVisible;

        public ActionsColumnViewModel()
        {
            ToggleCommand = ReactiveCommand.Create(() =>
            {
                IsButtonVisible = !IsButtonVisible;
            });
        }

        public bool IsButtonVisible
        {
            get => _isButtonVisible;
            set => this.RaiseAndSetIfChanged(ref _isButtonVisible, value);
        }
        
        public ICommand ToggleCommand { get; }
    }
    public partial class ActionsColumnView : UserControl
    {
        public ActionsColumnView()
        {
            InitializeComponent();
        }
    }
}

