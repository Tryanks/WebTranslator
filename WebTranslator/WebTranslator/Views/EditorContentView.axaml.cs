using System;
using System.IO;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using WebTranslator.Services;
using WebTranslator.ViewModels;

namespace WebTranslator.Views;

public partial class EditorContentView : UserControl
{
    private readonly XshdSyntaxDefinition _xsd;

    public EditorContentView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, HandleShortcutKeyDown, RoutingStrategies.Tunnel);
        // ReSharper disable once StringLiteralTypo
        const string xsdText = """
                               <?xml version="1.0"?>
                               <SyntaxDefinition name="MCTranslator" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
                                   <Color name="Color_0" foreground="#FF000000" background=""/>
                                   <Color name="Color_1" foreground="#FF0000AA" background=""/>
                                   <Color name="Color_2" foreground="#FF00AA00" background=""/>
                                   <Color name="Color_3" foreground="#FF00AAAA" background=""/>
                                   <Color name="Color_4" foreground="#FFAA0000" background=""/>
                                   <Color name="Color_5" foreground="#FFAA00AA" background=""/>
                                   <Color name="Color_6" foreground="#FFFFAA00" background=""/>
                                   <Color name="Color_7" foreground="#FFAAAAAA" background=""/>
                                   <Color name="Color_8" foreground="#FF555555" background=""/>
                                   <Color name="Color_9" foreground="#FF5555FF" background=""/>
                                   <Color name="Color_a" foreground="#FF55FF55" background=""/>
                                   <Color name="Color_b" foreground="#FF55FFFF" background=""/>
                                   <Color name="Color_c" foreground="#FFFF5555" background=""/>
                                   <Color name="Color_d" foreground="#FFFF55FF" background=""/>
                                   <Color name="Color_e" foreground="#FFFFFF55" background=""/>
                                   <Color name="Color_f" foreground="#FFFFFFFF" background=""/>
                                  
                                   <Color name="Color_l" fontWeight="bold"/>
                                   <Color name="Color_o" fontStyle="italic"/>
                                  
                                   <Color name="Format" fontWeight="bold" foreground="#FFFFD455"/>
                               
                                   <RuleSet>
                                       <Rule color="Color_0">§0(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_1">§1(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_2">§2(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_3">§3(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_4">§4(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_5">§5(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_6">§6(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_7">§7(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_8">§8(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_9">§9(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_a">§a(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_b">§b(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_c">§c(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_d">§d(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_e">§e(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_f">§f(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       
                                       <Rule color="Color_l">§l(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       <Rule color="Color_o">§o(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
                                       
                                       <Rule color="Format">%(\d+\$)?[a-hs%]</Rule>
                                   </RuleSet>
                               </SyntaxDefinition>
                               """;
        var reader = new XmlTextReader(new StringReader(xsdText));
        _xsd = HighlightingLoader.LoadXshd(reader);
        reader.Close();
    }

    private void HandleShortcutKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not EditorContentViewModel vm) return;
        if (HandleCommandShortcut(vm, e)) return;

        if (!e.KeyModifiers.HasFlag(KeyModifiers.Alt)) return;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta)) return;

        var number = GetDigit(e.Key);
        if (number is null) return;

        e.Handled = vm.ApplySuggestionByShortcut(number.Value);
    }

    private static bool HandleCommandShortcut(EditorContentViewModel vm, KeyEventArgs e)
    {
        if (e.Handled) return true;

        var handled = e.Key switch
        {
            Key.S when ShortcutDisplayService.IsCommandShortcut(e.KeyModifiers) => Execute(vm.SaveAll),
            Key.C when ShortcutDisplayService.IsCommandShortcut(e.KeyModifiers, shift: true) =>
                Execute(vm.TranslationItem.Clear),
            Key.N when ShortcutDisplayService.IsCommandShortcut(e.KeyModifiers, shift: true) => Execute(vm.NextItem),
            Key.B when ShortcutDisplayService.IsCommandShortcut(e.KeyModifiers, shift: true) => Execute(vm.PrevItem),
            Key.N when ShortcutDisplayService.IsCommandShortcut(e.KeyModifiers) => Execute(vm.SkipItem),
            _ => false
        };

        e.Handled = handled;
        return handled;
    }

    private static bool Execute(Action action)
    {
        action();
        return true;
    }

    private static int? GetDigit(Key key)
    {
        return key switch
        {
            Key.D1 or Key.NumPad1 => 1,
            Key.D2 or Key.NumPad2 => 2,
            Key.D3 or Key.NumPad3 => 3,
            Key.D4 or Key.NumPad4 => 4,
            Key.D5 or Key.NumPad5 => 5,
            Key.D6 or Key.NumPad6 => 6,
            Key.D7 or Key.NumPad7 => 7,
            Key.D8 or Key.NumPad8 => 8,
            Key.D9 or Key.NumPad9 => 9,
            _ => null
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SyntaxEditorLoaded(object? sender, RoutedEventArgs _)
    {
        if (sender is not TextEditor editor) return;
        editor.SyntaxHighlighting = HighlightingLoader.Load(_xsd, HighlightingManager.Instance);
        if (editor.Name != "SyntaxEditor2") return;
        editor.TextArea.TextEntered += (_, e) =>
        {
            if (e.Text != "%") return;
            if (DataContext is not EditorContentViewModel vm) return;
            var suggestions = vm.TranslationItem?.FormatSuggestions ?? [];
            if (suggestions.Count == 0) return;

            var completionWindow = new CompletionWindow(editor.TextArea);
            foreach (var suggestion in suggestions)
                completionWindow.CompletionList.CompletionData.Add(new FormatCompletionData(suggestion));
            completionWindow.Show();
        };
    }
}

internal class FormatCompletionData(string text) : ICompletionData
{
    public IImage? Image => null;
    public string Text { get; } = text;
    public object Content => Text;
    public object Description => $"插入 {Text}";
    public double Priority => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }
}
