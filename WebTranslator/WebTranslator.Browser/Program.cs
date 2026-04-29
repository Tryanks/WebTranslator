using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Media;
using WebTranslator;
using WebTranslator.Services;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        ShortcutDisplayService.SetUseAppleCommandModifier(BrowserShortcutPlatform.UsesAppleCommandModifier());
        DictionaryService.SetStorage(new BrowserDictionaryStorage());
        DictionaryService.SetRemoteSource(new BrowserHostedDictionaryRemoteSource());
        await DictionaryService.InitializeAsync();
        await BuildAvaloniaApp().StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .With(new FontManagerOptions
            {
                DefaultFamilyName = "avares://WebTranslator/Assets/SourceHanSansCN.otf#Source Han Sans CN"
            });
}
