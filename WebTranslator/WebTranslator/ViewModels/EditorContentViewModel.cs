using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;
using WebTranslator.Services;

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
            EditorList.Add(new EditorListItem(value.OriginalText,
                value.TranslatedText,
                value.Key,
                SubmitItem));
        }

        AllCount = EditorList.Count;
        TranslatedCount = EditorList.Count(x => x.IsTranslated);

        ExtensionMethods.Subscribe(this.WhenAnyValue(x => x.SelectedIndex),
            _ => TranslatedCount = EditorList.Count(x => x.IsTranslated));

        if (EditorList.Count > 0)
            SelectedIndex = 0;
    }

    public ModDictionary ModDictionary { get; set; }

    [Reactive] public string Header { get; set; }
    [Reactive] public ObservableCollection<EditorListItem> EditorList { get; set; } = [];
    [Reactive] public int SelectedIndex { get; set; } = -1;
    [Reactive] public EditorListItem TranslationItem { get; set; } = null!;
    [Reactive] public int TranslatedCount { get; set; }
    public int AllCount { get; set; }
    [Reactive] public bool AutoSkip { get; set; } = true;

    public void SaveAll()
    {
        foreach (var item in EditorList)
            if (item.IsChanged)
                item.Save();
    }

    public void ClearAllTranslationText()
    {
        foreach (var item in EditorList)
            if (item.SameText)
                item.Clear();
    }

    public void FillAllTranslationText()
    {
        foreach (var item in EditorList)
        {
            if (item.IsTranslated) continue;
            item.TranslatedText = item.OriginalText;
            item.Save();
        }
    }

    public void SaveItem()
    {
        TranslationItem.Save();
    }

    public void SubmitItem()
    {
        SaveItem();
        SkipItem();
    }

    public void NextItem()
    {
        if (SelectedIndex == -1) return;
        if (SelectedIndex == EditorList.Count - 1) return;
        SelectedIndex++;
    }

    public void SkipItem()
    {
        if (SelectedIndex == -1) return;
        if (SelectedIndex == EditorList.Count - 1) return;
        if (AutoSkip)
        {
            var index = SelectedIndex;
            do
            {
                index++;
            } while (index < EditorList.Count - 1 && !EditorList[index].IsEmpty);

            if (EditorList[index].IsEmpty)
            {
                SelectedIndex = index;
            }
            else
            {
                index = -1;
                do
                {
                    index++;
                } while (index < SelectedIndex && !EditorList[index].IsEmpty);

                if (EditorList[index].IsEmpty)
                {
                    SelectedIndex = index;
                }
                else
                {
                    SelectedIndex = EditorList.Count - 1;
                    ToastService.Notify("看起来没有尚未翻译的条目了");
                }
            }

            return;
        }

        NextItem();
    }

    public void PrevItem()
    {
        if (SelectedIndex <= 0) return;
        SelectedIndex--;
    }

    public void ExportCommand()
    {
        foreach (var item in EditorList)
        {
            if (item.IsChanged)
                item.Save();
            ModDictionary.TextDictionary[item.Key].TranslatedText = item.SavedTranslatedText;
        }

        NavigationService.NavigatePage(2, ModDictionary);
    }
}

public class EditorListItem : ViewModelBase
{
    public EditorListItem(string original, string translated, string key, Action act)
    {
        SubmitRequested = act;
        TranslatedDoc.TextChanged += (_, _) => TranslatedText = TranslatedDoc.Text;
        ExtensionMethods.Subscribe(this.WhenAnyValue(x => x.TranslatedText), s =>
        {
            if (s != TranslatedDoc.Text) TranslatedDoc.Text = s ?? "";
            IsTranslated = s != OriginalText && !string.IsNullOrEmpty(SavedTranslatedText);
            IsChanged = s != SavedTranslatedText;
            if (FormatMode != FormatMode.None)
            {
                var translatedFormats = FormatStringHelper.ExtractFormatStrings(s ?? "");
                FormatError = false;

                if (FormatMode == FormatMode.Sorted)
                {
                    var sortedTranslatedFormats = translatedFormats.OrderBy(f => f).ToList();
                    if (OriginalFormatStrings.SequenceEqual(sortedTranslatedFormats)) return;
                    FormatError = true;
                }
                else
                {
                    if (OriginalFormatStrings.SequenceEqual(translatedFormats)) return;
                    FormatError = true;
                }
            }

            TranslatedLineCount = TranslatedDoc.LineCount;
            EqualLines = OriginalLineCount <= TranslatedLineCount;
        });

        var cancel = false;
        TranslatedDoc.Changing += (_, e) =>
        {
            cancel = e.InsertedText.Text is "\r\n" or "\n" && EqualLines;
            if (e.InsertedText.Text is "%")
            {
            }
        };
        TranslatedDoc.UpdateFinished += (_, _) =>
        {
            if (!cancel) return;
            TranslatedDoc.UndoStack.Undo();
            TranslatedDoc.UndoStack.ClearRedoStack();
            cancel = false;
            SubmitRequested.Invoke();
        };

        OriginalDoc.Text = original;
        OriginalDoc.UndoStack.ClearAll();
        OriginalLineCount = OriginalDoc.LineCount;
        FormatMode = FormatStringHelper.DetermineFormatMode(original);
        if (FormatMode != FormatMode.None)
            OriginalFormatStrings = FormatStringHelper.ExtractFormatStrings(original).OrderBy(f => f).ToList();
        SavedTranslatedText = translated;
        TranslatedText = translated;
        TranslatedDoc.UndoStack.ClearAll();
        Key = key;
    }

    private Action SubmitRequested { get; }

    public string Key { get; }
    private FormatMode FormatMode { get; }
    private List<string> OriginalFormatStrings { get; } = [];
    [Reactive] public bool FormatError { get; set; }

    public TextDocument OriginalDoc { get; set; } = new();
    public TextDocument TranslatedDoc { get; set; } = new();
    public string OriginalText => OriginalDoc.Text;
    [Reactive] public string TranslatedText { get; set; }
    [Reactive] public string SavedTranslatedText { get; set; }
    [Reactive] public bool IsTranslated { get; set; }
    [Reactive] public bool IsChanged { get; set; }
    public int OriginalLineCount { get; }
    [Reactive] public int TranslatedLineCount { get; set; }
    [Reactive] public bool EqualLines { get; set; }

    public bool SameText => OriginalText == TranslatedText;
    public bool IsEmpty => IsChanged || string.IsNullOrEmpty(SavedTranslatedText);

    public void Save()
    {
        SavedTranslatedText = TranslatedText.Replace("\r\n", "\n");
        IsChanged = false;
        IsTranslated = SavedTranslatedText != OriginalText && !string.IsNullOrEmpty(SavedTranslatedText);
        TranslatedDoc.UndoStack.ClearAll();
    }

    public void Reset()
    {
        TranslatedText = SavedTranslatedText;
    }

    public void Clear()
    {
        TranslatedText = "";
        TranslatedDoc.UndoStack.ClearAll();
    }
}