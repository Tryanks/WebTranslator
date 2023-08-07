using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;

namespace WebTranslator.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // .WithInterFont()
            .With(new FontManagerOptions
            {
                FontFallbacks = new[]
                {
                    new FontFallback
                    {
                        FontFamily = new FontFamily("avares://WebTranslator/Assets/SourceHanSansCN-Regular.otf#Source Han Sans CN"),
                    }
                }
            })
            .LogToTrace()
            .UseReactiveUI();
}