using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebTranslator.Services;

public static class DialogService
{
    private static readonly Dictionary<string, Func<bool, Task<object>>> _dialogCallbacks = new();

    public static void RegisterDialog(string dialogName, Func<bool, Task<object>> callback)
    {
        _dialogCallbacks[dialogName] = callback;
    }

    public static void UnregisterDialog(string dialogName)
    {
        _dialogCallbacks.Remove(dialogName);
    }

    public static async Task ShowDialogAsync(string dialogName)
    {
        if (_dialogCallbacks.TryGetValue(dialogName, out var callback))
            await callback(true);
    }
}