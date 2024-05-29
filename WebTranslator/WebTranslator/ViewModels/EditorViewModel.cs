using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class EditorViewModel : ViewModelBase
{
    public EditorViewModel()
    {
        EditorPages.WhenAnyValue(x => x.Count)
            .Subscribe(x => IsEmpty = x == 0);
    }

    [Reactive] public bool IsEmpty { get; set; } = true;
    [Reactive] public ObservableCollection<EditorPageModel> EditorPages { get; set; } = new();
    [Reactive] public EditorPageModel? SelectedEditorPage { get; set; }

    public override void SetParameter(object? parameter)
    {
        if (parameter is not ModDictionary modDictionary) return;
        string header;
        if (modDictionary.CurseForgeId == "" && modDictionary.ModNamespace != "")
            header = modDictionary.ModNamespace;
        else if (modDictionary.CurseForgeId != "" && modDictionary.ModNamespace == "")
            header = modDictionary.CurseForgeId;
        else if (modDictionary.CurseForgeId != "" && modDictionary.ModNamespace != "")
            header = $"{modDictionary.CurseForgeId} / {modDictionary.ModNamespace}";
        else
            header = "New Tab";
        var page = new EditorPageModel
        {
            Header = header
        };
        foreach (var (_, value) in modDictionary.TextDictionary)
            page.AddItem(value.OriginalText, value.TranslatedText, value.Key);

        EditorPages.Add(page);
        SelectedEditorPage = page;
    }
}

public class EditorPageModel : ViewModelBase
{
    public EditorPageModel()
    {
        this.WhenAnyValue(x => x.SelectedIndex).Subscribe(x =>
        {
            if (x == -1) return;
            var editor = EditorList[x];
            SourceText = editor.EnText;
            TransText = editor.ZhText;
            KeyText = editor.Key;
        });
    }

    [Reactive] public string Header { get; set; } = "New Tab";

    [Reactive] public TextDocument SourceDoc { get; set; } = new();
    [Reactive] public TextDocument TransDoc { get; set; } = new();
    [Reactive] public ObservableCollection<EditorListItem> EditorList { get; set; } = new();
    [Reactive] public int SelectedIndex { get; set; } = -1;

    public string SourceText
    {
        get => SourceDoc.Text;
        set => SourceDoc.Text = value;
    }

    public string TransText
    {
        get => TransDoc.Text;
        set => TransDoc.Text = value;
    }

    [Reactive] public string KeyText { get; set; } = "";

    public void AddItem(string en, string zh, string key)
    {
        EditorList.Add(new EditorListItem(en, zh, key));
        if (SelectedIndex == -1)
            SelectedIndex = 0;
    }

    public void UpdateItem()
    {
        if (SelectedIndex <= 0 || SelectedIndex >= EditorList.Count - 1) return;
        var editor = EditorList[SelectedIndex];
        editor.ZhText = TransText;
        SelectNextItem();
    }

    public bool SelectNextItem()
    {
        if (SelectedIndex == -1) return false;
        if (SelectedIndex == EditorList.Count - 1) return false;
        SelectedIndex++;
        return true;
    }

    public bool SelectPrevItem()
    {
        if (SelectedIndex <= 0) return false;
        SelectedIndex--;
        return true;
    }
}

public class EditorListItem : ViewModelBase
{
    public readonly string Key;

    public EditorListItem(string en, string zh, string key)
    {
        EnText = en;
        this.WhenAnyValue(x => x.ZhText)
            .Subscribe(s => IsTranslated = s != EnText && !string.IsNullOrEmpty(s));
        ZhText = zh;
        Key = key;
    }

    [Reactive] public string EnText { get; set; }
    [Reactive] public string ZhText { get; set; }
    [Reactive] public bool IsTranslated { get; set; }
}