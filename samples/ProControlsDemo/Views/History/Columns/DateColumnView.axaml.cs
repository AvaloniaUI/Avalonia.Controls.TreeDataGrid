using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProControlsDemo.Views.History.Columns
{
    public class DateColumnView : UserControl
    {
        public DateColumnView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

