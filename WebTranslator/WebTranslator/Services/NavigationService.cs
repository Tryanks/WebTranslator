using System;
using System.Collections.Generic;

namespace WebTranslator.Services;

public static class NavigationService
{
    private static readonly List<Action<uint, object?>> Callback = [];

    public static void Register(Action<uint, object?> callback)
    {
        Callback.Add(callback);
    }

    public static void NavigatePage(uint page, object? parameter = null)
    {
        foreach (var action in Callback) action(page, parameter);
    }
}