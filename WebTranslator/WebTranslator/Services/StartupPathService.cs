using System;
using System.Linq;

namespace WebTranslator.Services;

public static class StartupPathService
{
    private static string? Path { get; set; }

    public static void Set(string[]? args)
    {
        Path = args?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }

    public static string? Take()
    {
        var path = Path;
        Path = null;
        return path;
    }
}
