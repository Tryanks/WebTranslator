using System;
using System.Collections.Generic;
using System.Diagnostics;
using ReactiveUI.Fody.Helpers;

namespace WebTranslator.Models;

public class ModDictionary(string curseForgeId, string modNamespace, MinecraftVersion version)
{
    public readonly SortedDictionary<string, TextElement> TextDictionary = new();

    private string _originalTemplate = "";
    public string CurseForgeId { get; } = curseForgeId;
    public string ModNamespace { get; } = modNamespace;

    public MinecraftVersion Version { get; } = version;
    public LangFormat Format => Version.GetFormat();

    public void LoadOriginalFile(string text)
    {
        var result = LangFileParser.Parse(text, Format);
        if (result.Dictionary.Count == 0)
            throw new ArgumentException("Empty list: Input text may not Minecraft Mod language file.");
        TextDictionary.Clear();
        foreach (var (key, value) in result.Dictionary)
        {
            var element = new TextElement(key, value.Text.Trim('"'), value.Template.Trim('"'));
            TextDictionary.Add(key, element);
        }

        _originalTemplate = result.Template;
    }

    public void LoadTranslatedFile(string text)
    {
        if (TextDictionary.Count == 0)
            throw new InvalidOperationException(
                "Translation text must be loaded after the original text has been loaded.");
        var result = LangFileParser.Parse(text, Version.GetFormat());
        foreach (var (key, value) in result.Dictionary)
        {
            if (!TextDictionary.TryGetValue(key, out var element))
            {
                Debug.WriteLine($"Warning: Redundant translation key: {key}, value: {value}, ignored.");
                continue;
            }

            element.TranslatedText = value.Text.Trim('"');
        }
    }

    public override string ToString()
    {
        var text = _originalTemplate;
        foreach (var (_, value) in TextDictionary)
        {
            var v = value.ToString();
            if (Format == LangFormat.Json) v = v.ReplaceToJson5();
            text = text.ReplaceOnce(value.ReplaceTemplate, v);
        }

        return text;
    }
}

public class TextElement(string k, string en, string template, string zh = "")
{
    public string Key { get; } = k;
    public string OriginalText { get; } = en;
    public string ReplaceTemplate { get; } = template;
    [Reactive] public string TranslatedText { get; set; } = zh;

    public override string ToString()
    {
        var value = string.IsNullOrEmpty(TranslatedText) ? OriginalText : TranslatedText;
        return value;
    }
}

public enum MinecraftVersion
{
    Version1Dot12Dot2, // 1.12.2
    Version1Dot16, // 1.16
    Version1Dot16Fabric, // 1.16-fabric
    Version1Dot18, // 1.18
    Version1Dot18Fabric, // 1.18-fabric
    Version1Dot19, // 1.19
    Version1Dot20, // 1.20
    Version1Dot20Fabric // 1.20-fabric
}

public enum LangFormat
{
    Json,
    Lang
}