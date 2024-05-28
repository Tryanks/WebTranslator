using System;

namespace WebTranslator.Services;

public static class NavigationService
{
    private static Action<uint>? _callback;

    public static void Register(Action<uint> callback)
    {
        _callback = callback;
    }

    public static void NavigatePage(uint page)
    {
        _callback?.Invoke(page);
    }
}