using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace WebTranslator.Services;

public static class ClipboardService
{
    private static IClipboard? Clipboard { get; set; }

    public static void Set(TopLevel? level)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (Clipboard is not null) return;
        Clipboard = level.Clipboard;
    }

    public static Task SetTextAsync(string? text)
    {
        ArgumentNullException.ThrowIfNull(Clipboard);
        return Clipboard.SetTextAsync(text);
    }
}
