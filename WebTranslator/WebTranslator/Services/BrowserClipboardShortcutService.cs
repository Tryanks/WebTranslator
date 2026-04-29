using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;

namespace WebTranslator.Services;

public static class BrowserClipboardShortcutService
{
    private static bool Registered { get; set; }

    public static void Register()
    {
        if (Registered || !OperatingSystem.IsBrowser()) return;

        InputElement.KeyDownEvent.AddClassHandler<TextBox>(HandleTextBoxKeyDown, RoutingStrategies.Tunnel);
        InputElement.KeyDownEvent.AddClassHandler<TextEditor>(HandleTextEditorKeyDown, RoutingStrategies.Tunnel);
        Registered = true;
    }

    private static void HandleTextBoxKeyDown(TextBox textBox, KeyEventArgs e)
    {
        if (!IsMetaShortcut(e)) return;

        var handled = e.Key switch
        {
            Key.C => Execute(textBox.Copy),
            Key.X when !textBox.IsReadOnly => Execute(textBox.Cut),
            Key.V when !textBox.IsReadOnly => Execute(textBox.Paste),
            Key.A => Execute(textBox.SelectAll),
            Key.Z when e.KeyModifiers.HasFlag(KeyModifiers.Shift) && textBox.CanRedo => Execute(textBox.Redo),
            Key.Z when textBox.CanUndo => Execute(textBox.Undo),
            _ => false
        };

        e.Handled = handled;
    }

    private static void HandleTextEditorKeyDown(TextEditor editor, KeyEventArgs e)
    {
        if (!IsMetaShortcut(e)) return;

        var handled = e.Key switch
        {
            Key.C => Execute(editor.Copy),
            Key.X when !editor.IsReadOnly => Execute(editor.Cut),
            Key.V when !editor.IsReadOnly => Execute(editor.Paste),
            Key.A => Execute(editor.SelectAll),
            Key.Z when e.KeyModifiers.HasFlag(KeyModifiers.Shift) && editor.CanRedo => Execute(editor.Redo),
            Key.Z when editor.CanUndo => Execute(editor.Undo),
            _ => false
        };

        e.Handled = handled;
    }

    private static bool IsMetaShortcut(KeyEventArgs e)
    {
        return !e.Handled && e.KeyModifiers.HasFlag(KeyModifiers.Meta);
    }

    private static bool Execute(Action action)
    {
        action();
        return true;
    }

    private static bool Execute(Func<bool> action)
    {
        return action();
    }
}
