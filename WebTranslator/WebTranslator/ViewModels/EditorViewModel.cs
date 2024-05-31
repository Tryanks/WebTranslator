using System.Collections.ObjectModel;
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
    [Reactive] public ObservableCollection<EditorContentViewModel> EditorPages { get; set; } = new();
    [Reactive] public EditorContentViewModel? SelectedEditorPage { get; set; }

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
        var page = new EditorContentViewModel(header, modDictionary);

        EditorPages.Add(page);
        SelectedEditorPage = page;
    }
}