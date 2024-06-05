using System;
using System.Text;
using System.Text.RegularExpressions;

namespace WebTranslator.Models;

public static class Helper
{
    public static LangFormat GetFormat(this MinecraftVersion version)
    {
        return version == MinecraftVersion.Version1Dot12Dot2 ? LangFormat.Lang : LangFormat.Json;
    }

    public static int ReplaceOnce(this StringBuilder original, string oldValue, string newValue, int startIndex)
    {
        var index = original.ToString().IndexOf(oldValue, startIndex, StringComparison.Ordinal);
        original.Replace(oldValue, newValue, index, oldValue.Length);
        return index;
    }

    public static void ReplaceMatchGroup(this StringBuilder original, Group oldValue, string newValue)
    {
        original.Remove(oldValue.Index, oldValue.Length);
        original.Insert(oldValue.Index, newValue);
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
        return '"' + s.Replace("\n", "\\n") + '"';
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

    public class WebTranslatorTemplate(int index = 0)
    {
        private const string Template = "|WebTranslator|Content%|WebTranslator|";

        public string Get()
        {
            return Template.Replace("%", index--.ToString("D6"));
        }
    }
}