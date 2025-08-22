using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace WebTranslator.Models;

public class ModDictionary(string curseForgeId, string modNamespace, MinecraftVersion version)
{
    public List<string> Keys = [];
    public Dictionary<string, TextElement> TextDictionary = new();

    public string CurseForgeId { get; } = curseForgeId;
    public string ModNamespace { get; } = modNamespace;

    public MinecraftVersion Version { get; } = version;
    public LangFormat Format => Version.GetFormat();

    public string Template { get; set; } = "";

    public static ModDictionary FromJson(string jsonString)
    {
        var dict = JsonConvert.DeserializeObject<ModDictionary>(jsonString);
        if (dict == null)
            throw new ArgumentException("Input text may not Minecraft Mod language file.");
        return dict;
    }

    public void LoadOriginalFile(string text)
    {
        var result = LangFileParser.Parse(text, Format);
        if (result.Dictionary.Count == 0)
            throw new ArgumentException("Empty list: Input text may not Minecraft Mod language file.");
        TextDictionary.Clear();
        foreach (var key in result.Keys)
        {
            Keys.Add(key);
            var value = result.Dictionary[key];
            var element = new TextElement(key, value.Text.Trim('"'), value.Template.Trim('"'));
            TextDictionary[key] = element;
        }

        Template = result.Template;
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
        var textBuilder = new StringBuilder(Template);
        foreach (var key in Keys)
        {
            var value = TextDictionary[key];
            var v = value.ToString();
            if (Format == LangFormat.Json) v = v.ReplaceToJson5();
            textBuilder.ReplaceOnce(value.ReplaceTemplate, v);
        }

        return textBuilder.ToString();
    }

    public bool Equals(ModDictionary? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return CurseForgeId == other.CurseForgeId && ModNamespace == other.ModNamespace && Version == other.Version
               && Template == other.Template && Keys.SequenceEqual(other.Keys);
    }

    public string Export()
    {
        var json = JsonConvert.SerializeObject(this);
        return json;
    }
}

public class TextElement(string k, string en, string template, string zh = "")
{
    public string Key { get; set; } = k;
    public string OriginalText { get; set; } = en;
    public string ReplaceTemplate { get; set; } = template;
    public string TranslatedText { get => field; set { if (value == field) return; field = value; } } = zh;

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