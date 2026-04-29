using System.Runtime.InteropServices;
using Avalonia.Input;

namespace WebTranslator.Services;

public static class ShortcutDisplayService
{
    private static bool UseAppleCommandModifier { get; set; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static string CommandModifier => UseAppleCommandModifier ? "⌘" : "Ctrl";
    public static string CommandShiftModifier => UseAppleCommandModifier ? "⌘⇧" : "Ctrl+Shift";
    public static string AltModifier => "Alt";

    public static void SetUseAppleCommandModifier(bool value)
    {
        UseAppleCommandModifier = value;
    }

    public static bool IsCommandShortcut(KeyModifiers modifiers, bool shift = false)
    {
        var commandModifier = UseAppleCommandModifier ? KeyModifiers.Meta : KeyModifiers.Control;
        var otherCommandModifier = UseAppleCommandModifier ? KeyModifiers.Control : KeyModifiers.Meta;

        if (!modifiers.HasFlag(commandModifier)) return false;
        if (modifiers.HasFlag(otherCommandModifier) || modifiers.HasFlag(KeyModifiers.Alt)) return false;
        return modifiers.HasFlag(KeyModifiers.Shift) == shift;
    }
}
