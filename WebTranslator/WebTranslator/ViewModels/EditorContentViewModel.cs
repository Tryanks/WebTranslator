using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class EditorContentViewModel : ViewModelBase
{
    public EditorContentViewModel(string header, ModDictionary md)
    {
        Header = header;
        ModDictionary = md;
        foreach (var (_, value) in md.TextDictionary)
        {
            if (string.IsNullOrEmpty(value.OriginalText)) continue;
            EditorList.Add(new EditorListItem(value.OriginalText, value.TranslatedText, value.Key));
        }

        if (EditorList.Count > 0)
            SelectedIndex = 0;

        this.WhenAnyValue(x => x.TranslationItem).Subscribe(x =>
        {
            if (x is null) return;
        });
    }

    public ModDictionary ModDictionary { get; set; }

    [Reactive] public string Header { get; set; }
    [Reactive] public ObservableCollection<EditorListItem> EditorList { get; set; } = [];
    [Reactive] public int SelectedIndex { get; set; } = -1;
    [Reactive] public EditorListItem TranslationItem { get; set; } = null!;

    public void SaveItem()
    {
        TranslationItem.Save();
    }

    public void SaveAndNextItem()
    {
        SaveItem();
        NextItem();
    }

    public void NextItem()
    {
        if (SelectedIndex == -1) return;
        if (SelectedIndex == EditorList.Count - 1) return;
        SelectedIndex++;
    }

    public void PrevItem()
    {
        if (SelectedIndex <= 0) return;
        SelectedIndex--;
    }
}

public class EditorListItem : ViewModelBase
{
    public EditorListItem(string original, string translated, string key)
    {
        TranslatedDoc.TextChanged += (_, _) => TranslatedText = TranslatedDoc.Text;
        this.WhenAnyValue(x => x.TranslatedText)
            .Subscribe(s =>
            {
                if (s != TranslatedDoc.Text) TranslatedDoc.Text = s ?? "";
                IsTranslated = s != OriginalText && !string.IsNullOrEmpty(s);
                IsChanged = s != SavedTranslatedText;
            });

        OriginalText = original;
        OriginalDoc.Text = original;
        OriginalDoc.UndoStack.ClearAll();
        SavedTranslatedText = translated;
        TranslatedText = translated;
        TranslatedDoc.UndoStack.ClearAll();
        Key = key;
    }

    public string Key { get; }

    [Reactive] public TextDocument OriginalDoc { get; set; } = new();
    [Reactive] public TextDocument TranslatedDoc { get; set; } = new();
    [Reactive] public string OriginalText { get; set; }
    [Reactive] public string TranslatedText { get; set; }
    [Reactive] public string SavedTranslatedText { get; set; }
    [Reactive] public bool IsTranslated { get; set; }
    [Reactive] public bool IsChanged { get; set; }

    public void Save()
    {
        SavedTranslatedText = TranslatedText;
        IsChanged = false;
    }

    public void Reset()
    {
        TranslatedText = SavedTranslatedText;
        IsChanged = false;
    }
}