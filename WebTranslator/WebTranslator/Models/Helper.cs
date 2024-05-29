using System;

namespace WebTranslator.Models;

public static class Helper
{
    public static LangFormat GetFormat(this MinecraftVersion version)
    {
        return version == MinecraftVersion.Version1Dot12Dot2 ? LangFormat.Lang : LangFormat.Json;
    }

    public static string ReplaceOnce(this string original, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(oldValue))
            throw new ArgumentException("The string to be replaced cannot be null or empty.", nameof(oldValue));

        var index = original.IndexOf(oldValue, StringComparison.Ordinal);
        if (index == -1) return original;

        return original[..index]
               + newValue
               + original[(index + oldValue.Length)..];
    }

    public static string ReplaceJson5(this string s)
    {
        return s
            .Replace("\r\n", "\n")
            .Replace("\\\n", "\n")
            .Replace("\\n", "\n");
    }

    public static string ReplaceToJson5(this string s)
    {
        return s.Replace("\n", "\\n");
    }

    public static MinecraftVersion GetVersion(string v)
    {
        return v switch
        {
            "1.12.2" => MinecraftVersion.Version1Dot12Dot2,
            "1.16" => MinecraftVersion.Version1Dot16,
            "1.16-fabric" => MinecraftVersion.Version1Dot16Fabric,
            "1.18" => MinecraftVersion.Version1Dot18,
            "1.18-fabric" => MinecraftVersion.Version1Dot18Fabric,
            "1.19" => MinecraftVersion.Version1Dot19,
            "1.20" => MinecraftVersion.Version1Dot20,
            "1.20-fabric" => MinecraftVersion.Version1Dot20Fabric,
            _ => throw new NotImplementedException()
        };
    }
}