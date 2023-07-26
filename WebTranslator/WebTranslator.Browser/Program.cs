using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using WebTranslator;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args) => await BuildAvaloniaApp()
        .UseReactiveUI()
        .With(new FontManagerOptions
        {
            FontFallbacks = new[]
            {
                new FontFallback
                {
                    FontFamily = new FontFamily("avares://WebTranslator/Assets/SourceHanSansCN.otf#Source Han Sans CN"),
                }
            }
        })
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}