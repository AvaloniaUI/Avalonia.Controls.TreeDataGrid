using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Threading;

namespace Avalonia.Controls.TreeDataGridTests
{
    public class TestWindow : Window
    {
        static TestWindow()
        {
            TemplateProperty.OverrideDefaultValue<TestWindow>(new FuncControlTemplate((p, _) => new ContentPresenter
            {
                [~ContentProperty] = p.GetObservable(ContentProperty).ToBinding()
            }));
        }
        
        public TestWindow(Size? clientSize = null)
        {
            Width = clientSize?.Width ?? 100;
            Height = clientSize?.Height ?? 100;

            IsVisible = true;
            // RunJobs is needed to apply template and update client size.
            Dispatcher.UIThread.RunJobs();
        }
        
        public TestWindow(Control child, Size? clientSize = null)
            : this(clientSize)
        {
            Content = child;
        }
    }
}
