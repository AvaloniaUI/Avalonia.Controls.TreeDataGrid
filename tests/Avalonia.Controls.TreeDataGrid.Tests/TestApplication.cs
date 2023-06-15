using Avalonia.Controls.TreeDataGridTests;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

[assembly: AvaloniaTestApplication(typeof(TestApplication))]

namespace Avalonia.Controls.TreeDataGridTests
{
    public class TestApplication : Application
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
    }
}
