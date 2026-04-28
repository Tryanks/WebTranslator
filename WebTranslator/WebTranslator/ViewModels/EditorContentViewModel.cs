using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ReactiveUI;
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
            _ =>
            {
                TranslatedCount = EditorList.Count(x => x.IsTranslated);
                RefreshSuggestions();
            });
        ExtensionMethods.Subscribe(this.WhenAnyValue(x => x.SearchQuery), _ => RefreshSearchResults());
        ExtensionMethods.Subscribe(this.WhenAnyValue(x => x.SelectedSearchItem), item =>
        {
            if (item is null) return;
            var index = EditorList.IndexOf(item);
            if (index >= 0) SelectedIndex = index;
            SearchQuery = "";
            SelectedSearchItem = null;
        });
        ExtensionMethods.Subscribe(this.WhenAnyValue(x => x.MarkSameText), enabled =>
        {
            foreach (var item in EditorList)
                item.MarkSameText = enabled;
        });

        if (EditorList.Count > 0)
            SelectedIndex = 0;
    }

    public ModDictionary ModDictionary { get; set; }

    public string Header { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public ObservableCollection<EditorListItem> EditorList { get => field; set { if (Equals(value, field)) return; field = value; this.RaisePropertyChanged(); } } = [];
    public int SelectedIndex { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } } = -1;
    public EditorListItem TranslationItem { get => field; set { if (Equals(value, field)) return; field = value; this.RaisePropertyChanged(); } } = null!;
    public ObservableCollection<EditorListItem> SearchResults { get; } = [];
    public ObservableCollection<TranslationSuggestion> Suggestions { get; } = [];
    public bool HasSuggestions { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public string SearchQuery { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } } = "";
    public bool HasSearchQuery { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public int SearchResultCount { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public EditorListItem? SelectedSearchItem { get => field; set { if (Equals(value, field)) return; field = value; this.RaisePropertyChanged(); } }
    public int TranslatedCount { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public int AllCount { get; set; }
    public bool AutoSkip { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } } = true;
    public bool MarkSameText { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }

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

    private void RefreshSearchResults()
    {
        SearchResults.Clear();
        var query = SearchQuery.Trim();
        HasSearchQuery = query.Length > 0;
        if (!HasSearchQuery)
        {
            SearchResultCount = 0;
            return;
        }

        foreach (var item in EditorList.Where(item =>
                     item.Key.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     item.OriginalText.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     item.SavedTranslatedText.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     item.TranslatedText.Contains(query, StringComparison.OrdinalIgnoreCase)))
            SearchResults.Add(item);

        SearchResultCount = SearchResults.Count;
    }

    private void RefreshSuggestions()
    {
        Suggestions.Clear();
        if (TranslationItem is null)
        {
            HasSuggestions = false;
            return;
        }

        foreach (var value in DictionaryService.GetTranslations(TranslationItem.OriginalText).Distinct())
            Suggestions.Add(new TranslationSuggestion(value, ApplySuggestion));
        HasSuggestions = Suggestions.Count > 0;
    }

    private void ApplySuggestion(string value)
    {
        if (TranslationItem is null) return;
        TranslationItem.TranslatedText = value;
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
                FormatSuggestions = GetMissingFormatStrings(translatedFormats);
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
            this.RaisePropertyChanged(nameof(IsSameTextMarked));
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
        {
            var formatStrings = FormatStringHelper.ExtractFormatStrings(original);
            OriginalFormatStrings = FormatMode == FormatMode.Sorted
                ? formatStrings.OrderBy(f => f).ToList()
                : formatStrings;
        }
        SavedTranslatedText = translated;
        TranslatedText = translated;
        TranslatedDoc.UndoStack.ClearAll();
        Key = key;
    }

    private Action SubmitRequested { get; }

    public string Key { get; }
    private FormatMode FormatMode { get; }
    private List<string> OriginalFormatStrings { get; } = [];
    public bool FormatError { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public IReadOnlyList<string> FormatSuggestions { get => field; set { if (Equals(value, field)) return; field = value; this.RaisePropertyChanged(); } } = [];

    public TextDocument OriginalDoc { get; set; } = new();
    public TextDocument TranslatedDoc { get; set; } = new();
    public string OriginalText => OriginalDoc.Text;
    public string TranslatedText { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public string SavedTranslatedText { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public bool IsTranslated { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public bool IsChanged { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public int OriginalLineCount { get; }
    public int TranslatedLineCount { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }
    public bool EqualLines { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); } }

    public bool SameText => OriginalText == TranslatedText;
    public bool MarkSameText { get => field; set { if (value == field) return; field = value; this.RaisePropertyChanged(); this.RaisePropertyChanged(nameof(IsSameTextMarked)); } }
    public bool IsSameTextMarked => MarkSameText && SameText;
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

    private IReadOnlyList<string> GetMissingFormatStrings(List<string> translatedFormats)
    {
        if (OriginalFormatStrings.Count == 0) return [];

        if (FormatMode == FormatMode.Format &&
            translatedFormats.Count < OriginalFormatStrings.Count &&
            OriginalFormatStrings.Take(translatedFormats.Count).SequenceEqual(translatedFormats))
            return [OriginalFormatStrings[translatedFormats.Count]];

        var missing = OriginalFormatStrings.ToList();
        foreach (var format in translatedFormats)
            missing.Remove(format);
        return missing;
    }
}

public class TranslationSuggestion(string text, Action<string> apply)
{
    public string Text { get; } = text;

    public void Apply()
    {
        apply(Text);
    }
}
