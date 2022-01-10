using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProControlsDemo.Views.History.Columns
{
    public class OutgoingColumnView : UserControl
    {
        public OutgoingColumnView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

