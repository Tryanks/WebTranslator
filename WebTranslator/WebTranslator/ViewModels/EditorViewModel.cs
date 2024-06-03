using ReactiveUI.Fody.Helpers;
using WebTranslator.Models;

namespace WebTranslator.ViewModels;

public class EditorViewModel : ViewModelBase
{
    [Reactive] public bool IsEmpty { get; set; } = true;
    [Reactive] public EditorContentViewModel EditorContent { get; set; } = null!;

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
        IsEmpty = false;
        EditorContent = page;
    }
}