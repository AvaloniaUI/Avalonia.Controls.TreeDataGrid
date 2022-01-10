using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProControlsDemo.Views.History.Columns
{
    public class BalanceColumnView : UserControl
    {
        public BalanceColumnView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

