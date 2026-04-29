using System.Runtime.InteropServices.JavaScript;

internal static partial class BrowserShortcutPlatform
{
    public static bool UsesAppleCommandModifier()
    {
        return GetPlatform().Contains("mac", System.StringComparison.OrdinalIgnoreCase) ||
               GetPlatform().Contains("iphone", System.StringComparison.OrdinalIgnoreCase) ||
               GetPlatform().Contains("ipad", System.StringComparison.OrdinalIgnoreCase);
    }

    [JSImport("globalThis.webTranslatorPlatform.platform")]
    private static partial string GetPlatform();
}
